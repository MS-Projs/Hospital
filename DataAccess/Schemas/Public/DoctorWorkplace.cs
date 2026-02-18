using DataAccess.Models;

namespace DataAccess.Schemas.Public;

public class DoctorWorkplace : Entity
{
    public long DoctorId { get; set; }
    public Doctor Doctor { get; set; } = default!;

    public string ClinicName { get; set; } = default!;
    public string ClinicAddress { get; set; } = default!;
}