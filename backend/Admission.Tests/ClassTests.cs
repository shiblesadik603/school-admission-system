using System.Net;
using System.Net.Http.Json;
using static Admission.Tests.ApiHelpers;

namespace Admission.Tests;

[Collection("Database")]
public class ClassTests : IAsyncLifetime
{
    private readonly TestApp _app;
    private readonly HttpClient _client;

    public ClassTests(TestDatabase db)
    {
        _app = new TestApp(db.ConnectionString);
        _client = _app.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _app.DisposeAsync();

    [Fact]
    public async Task Create_class_returns_201_and_shows_all_seats_free()
    {
        var response = await _client.PostAsJsonAsync("/classes", new { name = "Class 6 - Morning " + Guid.NewGuid(), totalSeats = 120 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ClassDto>();
        Assert.Equal(120, created!.TotalSeats);

        var details = await GetClass(_client, created.Id);
        Assert.Equal(0, details.HeldSeats);
        Assert.Equal(0, details.ConfirmedSeats);
        Assert.Equal(120, details.AvailableSeats);
    }

    [Theory]
    [InlineData("", 10)]
    [InlineData("Class 7", 0)]
    [InlineData("Class 7", -5)]
    public async Task Create_class_with_bad_input_returns_400(string name, int seats)
    {
        var response = await _client.PostAsJsonAsync("/classes", new { name, totalSeats = seats });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
        Assert.False(string.IsNullOrEmpty(error!.Message));
    }

    [Fact]
    public async Task Unknown_class_returns_404()
    {
        var response = await _client.GetAsync("/classes/c_does_not_exist");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
