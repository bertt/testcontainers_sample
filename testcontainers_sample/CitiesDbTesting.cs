using NUnit.Framework;
using DotNet.Testcontainers.Builders;
using System.Diagnostics;
using DotNet.Testcontainers.Containers;

namespace ConsoleAppTestContainers;
public class CitiesDbTesting
{
    private const string UnixSocketAddr = "unix:/var/run/docker.sock";
    private IContainer containerPostgres;

    [OneTimeSetUp]
    public async Task Setup()
    {
        var dockerEndpoint = Environment.GetEnvironmentVariable("DOCKER_HOST") ?? UnixSocketAddr;

        var image = new ImageFromDockerfileBuilder()
     .WithDockerfile("Dockerfile")
     .Build();

        await image.CreateAsync().ConfigureAwait(false);

        containerPostgres = new ContainerBuilder()
                .WithDockerEndpoint(dockerEndpoint)
                .WithImage(image)
                .WithEnvironment("POSTGRES_PASSWORD", "postgres")
                .WithPortBinding(5437, 5432)
                // .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .Build();

        await containerPostgres.StartAsync().ConfigureAwait(false);
        // wait 5 seconds for the container to start
        await Task.Delay(5000);
    }

    [OneTimeTearDown]
    public async Task TeardownOnce()
    {
        await containerPostgres.StopAsync();
        await containerPostgres.DisposeAsync(); //important for the event to cleanup to be fired!
    }

    [Test]
    public void TestCities()
    {
        Debug.WriteLine("testing started with ip " + containerPostgres.Hostname);
        var connectionString = $"Host={containerPostgres.Hostname};Username=postgres;Password=postgres;Port=5437";
        var connection = new Npgsql.NpgsqlConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = "select id, city_name, st_x(geom) as longitude, st_y(geom) as latitude from cities";
        command.CommandText = sql;
        var reader = command.ExecuteReader();
        var cities = new List<City>();  
        while (reader.Read())
        {
            cities.Add(new City
            {
                Id = reader.GetInt32(0),
                CityName = reader.GetString(1),
                Longitude = reader.GetDouble(2),
                Latitude = reader.GetDouble(3)
            });
        }
        connection.Close();

        Assert.That(cities.Count.Equals(5));
    }
}
