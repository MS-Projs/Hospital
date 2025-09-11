namespace Domain.Enums;

public enum ErrorEnum : short
{
    InternalServerError = 600,
    PhoneAlreadyExists,
    UserNotFound,
    DoctorNotFound,
    DoctorAlreadyExist,
    PatientAlreadyExist,
    PatientNotFound
}