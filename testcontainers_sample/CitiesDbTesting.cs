using DotNet.Testcontainers.Builders;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace ConsoleAppTestContainers;
public class CitiesDbTesting
{
    private PostgreSqlContainer containerPostgres;

    [OneTimeSetUp]
    public async Task Setup()
    {
        containerPostgres = new PostgreSqlBuilder()
        .WithImage("postgis/postgis:16-3.4-alpine")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

        await containerPostgres.StartAsync().ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task TeardownOnce()
    {
        await containerPostgres.StopAsync();
        await containerPostgres.DisposeAsync(); 
    }

    [Test]
    public void TestCities()
    {
        var connectionString = containerPostgres.GetConnectionString();
        var connection = new Npgsql.NpgsqlConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        var sql = "SELECT PostGIS_Version()";
        command.CommandText = sql;
        var version = command.ExecuteScalar();

        Assert.That(version.Equals("3.4 USE_GEOS=1 USE_PROJ=1 USE_STATS=1"));
        connection.Close();
    }
}
