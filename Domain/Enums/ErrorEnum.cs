namespace Domain.Enums;

public enum ErrorEnum : short
{
    BadRequest = 0,
    InternalServerError = 600,
    PhoneAlreadyExists,
    UserNotFound,
    DoctorNotFound,
    DoctorAlreadyExist,
    PatientAlreadyExist,
    PatientNotFound,
    CertificateTypeNotFound
}