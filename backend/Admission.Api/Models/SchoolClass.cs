namespace Admission.Api.Models;

// Every shift is its own class, e.g. "Class 6 - Morning Shift" and "Class 6 - Day Shift".
public class SchoolClass
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string NormalizedName { get; set; } = "";   // upper-case name, used for the unique index
    public int TotalSeats { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<Hold> Holds { get; set; } = new();
}
