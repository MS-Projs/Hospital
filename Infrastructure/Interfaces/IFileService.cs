using Domain.Models.Common;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Interfaces;

public interface IFileService
{
    Task<Result<string>> SaveFileAsync(IFormFile file, string subfolder);
    Task<Result<(Stream stream, string fileName, string contentType)>> GetFileAsync(string relativePath);
    Task<Result<bool>> DeleteFileAsync(string relativePath);
    string GenerateEncryptionKey();
}