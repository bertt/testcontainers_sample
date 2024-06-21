using NUnit.Framework;
using DotNet.Testcontainers.Builders;

namespace ConsoleAppTestContainers;
public class CitiesDbTesting
{
    private string hostname;
    [SetUp]
    public async Task Setup()
    {
        var image = new ImageFromDockerfileBuilder()
     .WithDockerfile("Dockerfile")
     .Build();

        await image.CreateAsync().ConfigureAwait(false);

        var containerPostgres = new ContainerBuilder()
                .WithImage(image)
                .WithName("citiesdb")
                .WithEnvironment("POSTGRES_PASSWORD", "postgres")
                .WithPortBinding(5437, 5432)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .Build();

        await containerPostgres.StartAsync().ConfigureAwait(false);

        hostname = containerPostgres.Hostname;
        Console.WriteLine("Hostname:" + hostname);

    }

    [Test]
    public void TestCities()
    {
        var connectionString = $"Host={hostname};Username=postgres;Password=postgres;Port=5437";
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
