using DataAccess.Models;

namespace DataAccess.Schemas.Public;

public class DoctorSpecialization : Entity
{
    public long DoctorId { get; set; }
    public Doctor Doctor { get; set; } = default!;

    public string Name { get; set; } = default!;
}