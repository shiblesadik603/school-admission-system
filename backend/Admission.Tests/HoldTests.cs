using System.Net;
using System.Net.Http.Json;
using static Admission.Tests.ApiHelpers;

namespace Admission.Tests;

[Collection("Database")]
public class HoldTests : IAsyncLifetime
{
    private readonly TestDatabase _db;
    private readonly TestApp _app;
    private readonly HttpClient _client;

    public HoldTests(TestDatabase db)
    {
        _db = db;
        _app = new TestApp(db.ConnectionString);
        _client = _app.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _app.DisposeAsync();

    [Fact]
    public async Task Apply_holds_a_seat_and_confirm_twice_counts_only_once()
    {
        var cls = await CreateClass(_client, 5);
        var student = NewStudentId();

        var applyResponse = await Apply(_client, cls.Id, student);
        Assert.Equal(HttpStatusCode.Created, applyResponse.StatusCode);
        var hold = (await applyResponse.Content.ReadFromJsonAsync<HoldDto>())!;
        Assert.Equal("HELD", hold.Status);
        Assert.Equal(student, hold.StudentId);

        var first = await Confirm(_client, hold.HoldId, student);
        var second = await Confirm(_client, hold.HoldId, student);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var seats = await GetClass(_client, cls.Id);
        Assert.Equal(1, seats.ConfirmedSeats);
        Assert.Equal(0, seats.HeldSeats);
        Assert.Equal(4, seats.AvailableSeats);
    }

    [Fact]
    public async Task Student_cannot_have_two_active_applications_in_same_class()
    {
        var cls = await CreateClass(_client, 5);
        var student = NewStudentId();

        var first = await Apply(_client, cls.Id, student);
        var second = await Apply(_client, cls.Id, student);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Apply_when_class_is_full_returns_409()
    {
        var cls = await CreateClass(_client, 1);

        var first = await Apply(_client, cls.Id, NewStudentId());
        var second = await Apply(_client, cls.Id, NewStudentId());

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        var error = await second.Content.ReadFromJsonAsync<ErrorDto>();
        Assert.Equal("NO_SEATS_AVAILABLE", error!.Error);
    }

    [Fact]
    public async Task Cancel_frees_the_seat()
    {
        var cls = await CreateClass(_client, 1);
        var student = NewStudentId();
        var hold = (await (await Apply(_client, cls.Id, student)).Content.ReadFromJsonAsync<HoldDto>())!;

        var cancel = await Cancel(_client, hold.HoldId, student);
        Assert.Equal(HttpStatusCode.NoContent, cancel.StatusCode);

        var other = await Apply(_client, cls.Id, NewStudentId());
        Assert.Equal(HttpStatusCode.Created, other.StatusCode);
    }

    [Fact]
    public async Task Only_owner_can_confirm()
    {
        var cls = await CreateClass(_client, 1);
        var hold = (await (await Apply(_client, cls.Id, NewStudentId())).Content.ReadFromJsonAsync<HoldDto>())!;

        var response = await Confirm(_client, hold.HoldId, NewStudentId());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Missing_or_wrong_student_id_returns_400()
    {
        var cls = await CreateClass(_client, 1);

        var noHeader = await _client.PostAsync($"/classes/{cls.Id}/holds", null);
        var shortId = await Apply(_client, cls.Id, "12345");

        Assert.Equal(HttpStatusCode.BadRequest, noHeader.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, shortId.StatusCode);
    }

    [Fact]
    public async Task Hold_expires_by_itself_and_seat_comes_back()
    {
        // separate app with a 2 second hold time
        await using var shortApp = new TestApp(_db.ConnectionString, holdSeconds: 2);
        var client = shortApp.CreateClient();

        var cls = await CreateClass(client, 1);
        var student = NewStudentId();
        var hold = (await (await Apply(client, cls.Id, student)).Content.ReadFromJsonAsync<HoldDto>())!;
        Assert.Equal(0, (await GetClass(client, cls.Id)).AvailableSeats);

        await Task.Delay(TimeSpan.FromSeconds(3));

        var status = await client.GetFromJsonAsync<HoldDto>($"/holds/{hold.HoldId}");
        Assert.Equal("EXPIRED", status!.Status);
        Assert.Equal(1, (await GetClass(client, cls.Id)).AvailableSeats);

        var confirm = await Confirm(client, hold.HoldId, student);
        Assert.Equal(HttpStatusCode.Conflict, confirm.StatusCode);
    }
}
