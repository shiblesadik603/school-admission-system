using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Admission.Tests;

// Runs the real API in memory against the test database.
public class TestApp : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly int _holdSeconds;

    public TestApp(string connectionString, int holdSeconds = 600)
    {
        _connectionString = connectionString;
        _holdSeconds = holdSeconds;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", _connectionString);
        builder.UseSetting("HOLD_DURATION_SECONDS", _holdSeconds.ToString());
    }
}
