namespace Domain.Models.API.Requests;

public record VerifySessionRequest(
    long SessionId,
    string Code);