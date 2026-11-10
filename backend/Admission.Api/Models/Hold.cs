namespace Admission.Api.Models;

// One application of a student for a class. While HELD the seat is reserved until ExpiresAt.
public class Hold
{
    public string Id { get; set; } = "";
    public string ClassId { get; set; } = "";
    public SchoolClass? Class { get; set; }
    public string StudentId { get; set; } = "";
    public HoldStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // The cleanup job runs every few seconds, so a row can still say HELD for a moment
    // after its time is up. Always use this when showing the status to someone.
    public HoldStatus GetCurrentStatus(DateTime now)
    {
        if (Status == HoldStatus.HELD && ExpiresAt <= now)
            return HoldStatus.EXPIRED;
        return Status;
    }
}
