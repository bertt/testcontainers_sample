using NUnit.Framework;
using DotNet.Testcontainers.Builders;
using System.Diagnostics;

namespace ConsoleAppTestContainers;
public class CitiesDbTesting
{
    private string? ip;

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
        ip = containerPostgres.Hostname;

        // wait 10 seconds more
        await Task.Delay(10000);


    }

    [Test]
    public void TestCities()
    {
        Debug.WriteLine("testing started with ip " + ip);
        var connectionString = $"Host={ip};Username=postgres;Password=postgres;Port=5437";
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
