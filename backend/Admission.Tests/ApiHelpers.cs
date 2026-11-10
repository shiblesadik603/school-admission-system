using System.Net.Http.Json;

namespace Admission.Tests;

public record ClassDto(string Id, string Name, int TotalSeats, int HeldSeats, int ConfirmedSeats, int AvailableSeats);
public record HoldDto(string HoldId, string ClassId, string StudentId, string Status, DateTime ExpiresAt);
public record ErrorDto(string Error, string Message);

public static class ApiHelpers
{
    private static int _studentCounter;

    // Unique 17 digit student id for each call
    public static string NewStudentId()
    {
        var n = Interlocked.Increment(ref _studentCounter);
        return (20150000000000000L + Random.Shared.Next(1, 1_000_000) * 1000L + n).ToString();
    }

    public static async Task<ClassDto> CreateClass(HttpClient client, int seats)
    {
        var response = await client.PostAsJsonAsync("/classes", new { name = "Class " + Guid.NewGuid(), totalSeats = seats });
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<ClassDto>();
        return created!;
    }

    public static Task<HttpResponseMessage> Apply(HttpClient client, string classId, string studentId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/classes/{classId}/holds");
        request.Headers.Add("X-Student-Id", studentId);
        return client.SendAsync(request);
    }

    public static Task<HttpResponseMessage> Confirm(HttpClient client, string holdId, string studentId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/holds/{holdId}/confirm");
        request.Headers.Add("X-Student-Id", studentId);
        return client.SendAsync(request);
    }

    public static Task<HttpResponseMessage> Cancel(HttpClient client, string holdId, string studentId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/holds/{holdId}");
        request.Headers.Add("X-Student-Id", studentId);
        return client.SendAsync(request);
    }

    public static async Task<ClassDto> GetClass(HttpClient client, string classId)
    {
        return (await client.GetFromJsonAsync<ClassDto>($"/classes/{classId}"))!;
    }
}
