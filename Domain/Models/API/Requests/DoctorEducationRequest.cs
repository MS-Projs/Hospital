namespace Domain.Models.API.Requests;

public record DoctorEducationRequest{
    public string DegreeType { get; set; } = default!; // Bachelor, Master, etc.
    public string University { get; set; } = default!;
    public string Country { get; set; } = default!;
    public int GraduationYear { get; set; }
    public string? Specialization { get; set; }
}