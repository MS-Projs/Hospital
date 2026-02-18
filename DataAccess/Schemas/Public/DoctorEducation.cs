using DataAccess.Models;

namespace DataAccess.Schemas.Public;

public class DoctorEducation : Entity
{
    public long DoctorId { get; set; }
    public Doctor Doctor { get; set; } = default!;

    public string DegreeType { get; set; } = default!; // Bachelor, Master
    public string University { get; set; } = default!;
    public string Country { get; set; } = default!;
    public int GraduationYear { get; set; }
    public string? Specialization { get; set; }
}