using System.Net;
using static Admission.Tests.ApiHelpers;

namespace Admission.Tests;

[Collection("Database")]
public class ConcurrencyTests
{
    private readonly TestDatabase _db;

    public ConcurrencyTests(TestDatabase db)
    {
        _db = db;
    }

    [Fact]
    public async Task Many_students_applying_at_once_never_get_more_seats_than_exist()
    {
        await using var app = new TestApp(_db.ConnectionString);
        var client = app.CreateClient();

        const int seats = 10;
        const int students = 60;
        var cls = await CreateClass(client, seats);

        // fire all requests at the same time
        var tasks = Enumerable.Range(0, students)
            .Select(_ => Apply(client, cls.Id, NewStudentId()))
            .ToList();
        var responses = await Task.WhenAll(tasks);

        var created = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflicts = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        Assert.Equal(seats, created);
        Assert.Equal(students - seats, conflicts);

        var result = await GetClass(client, cls.Id);
        Assert.Equal(seats, result.HeldSeats);
        Assert.Equal(0, result.AvailableSeats);
    }

    [Fact]
    public async Task Two_backend_instances_on_same_database_do_not_oversell()
    {
        // two separate app instances, like two backend processes on different ports
        await using var app1 = new TestApp(_db.ConnectionString);
        await using var app2 = new TestApp(_db.ConnectionString);
        var client1 = app1.CreateClient();
        var client2 = app2.CreateClient();

        const int seats = 7;
        var cls = await CreateClass(client1, seats);

        var tasks = Enumerable.Range(0, 50)
            .Select(i => Apply(i % 2 == 0 ? client1 : client2, cls.Id, NewStudentId()))
            .ToList();
        var responses = await Task.WhenAll(tasks);

        Assert.Equal(seats, responses.Count(r => r.StatusCode == HttpStatusCode.Created));

        var result = await GetClass(client2, cls.Id);
        Assert.True(result.HeldSeats + result.ConfirmedSeats <= result.TotalSeats);
        Assert.Equal(seats, result.HeldSeats);
    }

    [Fact]
    public async Task Same_student_clicking_apply_many_times_gets_only_one_hold()
    {
        await using var app = new TestApp(_db.ConnectionString);
        var client = app.CreateClient();
        var cls = await CreateClass(client, 5);
        var student = NewStudentId();

        var responses = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => Apply(client, cls.Id, student)));

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(1, (await GetClass(client, cls.Id)).HeldSeats);
    }
}
