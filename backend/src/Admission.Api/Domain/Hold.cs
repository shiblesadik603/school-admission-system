namespace Admission.Api.Domain;

/// <summary>An application (seat hold) of one student for one class.</summary>
public class Hold
{
    public string Id { get; set; } = default!;
    public string ClassId { get; set; } = default!;
    public SchoolClass Class { get; set; } = default!;

    /// <summary>17 digit birth registration number of the student.</summary>
    public string StudentId { get; set; } = default!;

    public HoldStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// The status as the outside world should see it. A HELD row whose time has passed is
    /// already EXPIRED even if the background sweeper has not updated the row yet.
    /// </summary>
    public HoldStatus EffectiveStatus(DateTime nowUtc) =>
        Status == HoldStatus.HELD && ExpiresAt <= nowUtc ? HoldStatus.EXPIRED : Status;
}
