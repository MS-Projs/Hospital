namespace Domain.Models.API.Results;

public record CreateSessionResult(
    long SessionId,
    DateTime ExpireDate);