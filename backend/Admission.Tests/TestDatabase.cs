using Testcontainers.PostgreSql;

namespace Admission.Tests;

// Starts one PostgreSQL container for all tests.
// Set TEST_DB_CONNECTION to use an existing database instead.
public class TestDatabase : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public string ConnectionString { get; private set; } = "";

    public async Task InitializeAsync()
    {
        var existing = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION");
        if (!string.IsNullOrEmpty(existing))
        {
            ConnectionString = existing;
            return;
        }

        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
            await _container.DisposeAsync();
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<TestDatabase> { }
