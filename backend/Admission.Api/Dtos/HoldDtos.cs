using Admission.Api.Models;

namespace Admission.Api.Dtos;

public record HoldResponse(
    string HoldId,
    string ClassId,
    string ClassName,
    string StudentId,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime ServerTime)
{
    public static HoldResponse FromHold(Hold hold, string className, DateTime now)
    {
        return new HoldResponse(
            hold.Id,
            hold.ClassId,
            className,
            hold.StudentId,
            hold.GetCurrentStatus(now).ToString(),
            hold.ExpiresAt,
            hold.CreatedAt,
            hold.ConfirmedAt,
            now);
    }
}

public record ConfirmHoldResponse(string HoldId, string Status);
