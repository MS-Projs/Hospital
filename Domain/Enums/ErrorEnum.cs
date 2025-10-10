namespace Domain.Enums;

public enum ErrorEnum
{
    BadRequest,
    UserNotFound,
    PhoneAlreadyExists,
    DoctorNotFound,
    DoctorAlreadyExist,
    PatientNotFound,
    PatientAlreadyExist,
    CertificateTypeNotFound,
    AppointmentNotFound,
    ReportNotFound,
    DocumentNotFound,
    CertificateNotFound,
    FileTooLarge,
    InvalidFileType,
    FileNotFound,
    UnauthorizedAccess,
    InternalServerError,
    DocumentCategoryNotFound
}