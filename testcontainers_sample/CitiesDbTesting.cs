using DotNet.Testcontainers.Builders;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace ConsoleAppTestContainers;
public class CitiesDbTesting
{
    private PostgreSqlContainer _containerPostgres;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _containerPostgres = new PostgreSqlBuilder()
        .WithImage("postgis/postgis:16-3.4-alpine")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();
        var initScript = File.ReadAllText("./postgres-db/create_cities.sql");

        await _containerPostgres.StartAsync().ConfigureAwait(false);
        await _containerPostgres.ExecScriptAsync(initScript).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task TeardownOnce()
    {
        await _containerPostgres.StopAsync();
        await _containerPostgres.DisposeAsync(); 
    }

    [Test]
    public void TestCities()
    {
        var connectionString = _containerPostgres.GetConnectionString();
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
        connection.Close();
    }
}
