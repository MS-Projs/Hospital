using DataAccess.Schemas.Auth;
using DataAccess.Schemas.Public;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public partial class EntityContext
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Session> Sessions { get; set; }
    public virtual DbSet<Patient> Patients { get; set; }
    public virtual DbSet<Doctor> Doctors { get; set; }
    public virtual DbSet<DoctorCertificate> DoctorCertificates { get; set; }
    public virtual DbSet<Report> Reports { get; set; }
    public virtual DbSet<Appointment> Appointments { get; set; }
    public virtual DbSet<PatientDocument> PatientDocuments { get; set; }

}