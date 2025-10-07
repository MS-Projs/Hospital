namespace Domain.Models.Infrastructure.Results;

public record FileViewModel(
    string Path,
    string OriginalName,
    long Size,
    string Extension);