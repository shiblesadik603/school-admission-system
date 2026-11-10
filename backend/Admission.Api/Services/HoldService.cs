using Admission.Api.Data;
using Admission.Api.Dtos;
using Admission.Api.Helpers;
using Admission.Api.Middleware;
using Admission.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Admission.Api.Services;

public class HoldService
{
    private readonly AppDbContext _db;
    private readonly int _holdDurationSeconds;

    public HoldService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _holdDurationSeconds = config.GetValue("HOLD_DURATION_SECONDS", 600);
    }

    public async Task<HoldResponse> CreateHold(string classId, string studentId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        // Lock the class row. Other requests for the same class wait here until we commit,
        // so two requests can never take the last seat at the same time. The lock is in
        // PostgreSQL, so this also works when more than one backend uses the same database.
        var schoolClass = await _db.Classes
            .FromSql($"SELECT * FROM classes WHERE id = {classId} FOR UPDATE")
            .FirstOrDefaultAsync();

        if (schoolClass == null)
            throw new ApiException(404, "CLASS_NOT_FOUND", "Class not found.");

        var now = DateTime.UtcNow;

        // Mark this class's timed-out holds as EXPIRED so they don't block a new application.
        await _db.Holds
            .Where(h => h.ClassId == classId && h.Status == HoldStatus.HELD && h.ExpiresAt <= now)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.Status, HoldStatus.EXPIRED));

        var existing = await _db.Holds
            .Where(h => h.ClassId == classId && h.StudentId == studentId)
            .Where(h => h.Status == HoldStatus.HELD || h.Status == HoldStatus.CONFIRMED)
            .FirstOrDefaultAsync();

        if (existing != null && existing.Status == HoldStatus.CONFIRMED)
            throw new ApiException(409, "ALREADY_ADMITTED", "This student is already admitted to this class.");
        if (existing != null)
            throw new ApiException(409, "ALREADY_APPLIED", "This student already has an active application for this class.");

        var usedSeats = await _db.Holds.CountAsync(h =>
            h.ClassId == classId && (h.Status == HoldStatus.HELD || h.Status == HoldStatus.CONFIRMED));

        if (usedSeats >= schoolClass.TotalSeats)
            throw new ApiException(409, "NO_SEATS_AVAILABLE", "Sorry, there are no seats available in this class right now.");

        var hold = new Hold
        {
            Id = IdGenerator.NewHoldId(),
            ClassId = classId,
            StudentId = studentId,
            Status = HoldStatus.HELD,
            CreatedAt = now,
            ExpiresAt = now.AddSeconds(_holdDurationSeconds)
        };
        _db.Holds.Add(hold);

        try
        {
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // shouldn't happen because of the lock above, the unique index is just a backup
            throw new ApiException(409, "ALREADY_APPLIED", "This student already has an active application for this class.");
        }

        return HoldResponse.FromHold(hold, schoolClass.Name, now);
    }

    public async Task<HoldResponse> GetHold(string holdId)
    {
        var hold = await _db.Holds
            .AsNoTracking()
            .Include(h => h.Class)
            .FirstOrDefaultAsync(h => h.Id == holdId);

        if (hold == null)
            throw new ApiException(404, "HOLD_NOT_FOUND", "Application not found.");

        return HoldResponse.FromHold(hold, hold.Class!.Name, DateTime.UtcNow);
    }

    public async Task<List<HoldResponse>> GetStudentHolds(string studentId)
    {
        var holds = await _db.Holds
            .AsNoTracking()
            .Include(h => h.Class)
            .Where(h => h.StudentId == studentId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

        var now = DateTime.UtcNow;
        return holds.Select(h => HoldResponse.FromHold(h, h.Class!.Name, now)).ToList();
    }

    public async Task<ConfirmHoldResponse> ConfirmHold(string holdId, string studentId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        // Lock the hold row so double clicks / parallel requests are handled one at a time.
        var hold = await LockHold(holdId, studentId);
        var now = DateTime.UtcNow;

        if (hold.Status == HoldStatus.CONFIRMED)
        {
            // Already paid. Return success again but don't count the seat twice.
            return new ConfirmHoldResponse(hold.Id, "CONFIRMED");
        }

        if (hold.Status == HoldStatus.CANCELLED)
            throw new ApiException(409, "HOLD_CANCELLED", "This application was cancelled.");

        if (hold.GetCurrentStatus(now) == HoldStatus.EXPIRED)
        {
            hold.Status = HoldStatus.EXPIRED;
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            throw new ApiException(409, "HOLD_EXPIRED", "The payment time for this application is over. Please apply again.");
        }

        hold.Status = HoldStatus.CONFIRMED;
        hold.ConfirmedAt = now;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return new ConfirmHoldResponse(hold.Id, "CONFIRMED");
    }

    public async Task CancelHold(string holdId, string studentId)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        var hold = await LockHold(holdId, studentId);
        var status = hold.GetCurrentStatus(DateTime.UtcNow);

        if (status == HoldStatus.CANCELLED)
            return; // already cancelled, nothing to do

        if (status == HoldStatus.CONFIRMED)
            throw new ApiException(409, "ALREADY_CONFIRMED", "Admission fee is already paid, this application can't be cancelled.");

        if (status == HoldStatus.EXPIRED)
            throw new ApiException(409, "HOLD_EXPIRED", "This application has already expired.");

        hold.Status = HoldStatus.CANCELLED;
        hold.CancelledAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private async Task<Hold> LockHold(string holdId, string studentId)
    {
        var hold = await _db.Holds
            .FromSql($"SELECT * FROM holds WHERE id = {holdId} FOR UPDATE")
            .FirstOrDefaultAsync();

        if (hold == null)
            throw new ApiException(404, "HOLD_NOT_FOUND", "Application not found.");
        if (hold.StudentId != studentId)
            throw new ApiException(403, "NOT_YOUR_APPLICATION", "This application belongs to another student.");

        return hold;
    }
}
