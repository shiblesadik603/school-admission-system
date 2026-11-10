namespace Admission.Api.Domain;

/// <summary>
/// One admission "class". Every shift is modelled as its own class
/// (e.g. "Class 6 - Morning Shift" and "Class 6 - Day Shift" are two classes).
/// </summary>
public class SchoolClass
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;

    /// <summary>Upper-cased, trimmed name used to enforce unique class names.</summary>
    public string NormalizedName { get; set; } = default!;

    public int TotalSeats { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<Hold> Holds { get; set; } = new();
}
