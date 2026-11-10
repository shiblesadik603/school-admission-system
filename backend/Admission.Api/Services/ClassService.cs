using Admission.Api.Data;
using Admission.Api.Dtos;
using Admission.Api.Helpers;
using Admission.Api.Middleware;
using Admission.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Admission.Api.Services;

public class ClassService
{
    private readonly AppDbContext _db;

    public ClassService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ClassResponse> CreateClass(CreateClassRequest request)
    {
        var name = request.Name?.Trim() ?? "";

        if (name == "")
            throw new ApiException(400, "INVALID_NAME", "Class name is required.");
        if (name.Length > 100)
            throw new ApiException(400, "INVALID_NAME", "Class name can be at most 100 characters.");
        if (request.TotalSeats == null)
            throw new ApiException(400, "INVALID_TOTAL_SEATS", "Total seats is required.");
        if (request.TotalSeats < 1 || request.TotalSeats > 100000)
            throw new ApiException(400, "INVALID_TOTAL_SEATS", "Total seats must be between 1 and 100000.");

        var schoolClass = new SchoolClass
        {
            Id = IdGenerator.NewClassId(),
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            TotalSeats = request.TotalSeats.Value,
            CreatedAt = DateTime.UtcNow
        };

        _db.Classes.Add(schoolClass);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ApiException(409, "CLASS_NAME_EXISTS", $"A class named \"{name}\" already exists.");
        }

        return new ClassResponse(schoolClass.Id, schoolClass.Name, schoolClass.TotalSeats);
    }

    public async Task<List<ClassSeatsResponse>> GetAllClasses()
    {
        var now = DateTime.UtcNow;

        // A HELD hold only counts while its time is not over. So a seat comes back
        // as soon as the hold expires, without waiting for any job or API call.
        var rows = await _db.Classes
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.TotalSeats,
                Held = c.Holds.Count(h => h.Status == HoldStatus.HELD && h.ExpiresAt > now),
                Confirmed = c.Holds.Count(h => h.Status == HoldStatus.CONFIRMED)
            })
            .ToListAsync();

        return rows
            .Select(r => new ClassSeatsResponse(r.Id, r.Name, r.TotalSeats, r.Held, r.Confirmed,
                r.TotalSeats - r.Held - r.Confirmed))
            .ToList();
    }

    public async Task<ClassSeatsResponse> GetClass(string classId)
    {
        var now = DateTime.UtcNow;

        var row = await _db.Classes
            .Where(c => c.Id == classId)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.TotalSeats,
                Held = c.Holds.Count(h => h.Status == HoldStatus.HELD && h.ExpiresAt > now),
                Confirmed = c.Holds.Count(h => h.Status == HoldStatus.CONFIRMED)
            })
            .FirstOrDefaultAsync();

        if (row == null)
            throw new ApiException(404, "CLASS_NOT_FOUND", "Class not found.");

        return new ClassSeatsResponse(row.Id, row.Name, row.TotalSeats, row.Held, row.Confirmed,
            row.TotalSeats - row.Held - row.Confirmed);
    }
}
