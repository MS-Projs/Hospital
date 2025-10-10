using System.Security.Cryptography;
using System.Text;
using Domain.Enums;
using Domain.Models.Common;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class FileService(ILogger<FileService> logger) : IFileService
{
    private const string FileStoragePath = "wwwroot/uploads";

    public async Task<Result<string>> SaveFileAsync(IFormFile file, string subfolder)
    {
        try
        {
            if (file == null || file.Length == 0)
                return new ErrorModel(ErrorEnum.BadRequest);

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
                return new ErrorModel(ErrorEnum.FileTooLarge);

            // Validate file extension
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return new ErrorModel(ErrorEnum.InvalidFileType);

            // Create directory if not exists
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), FileStoragePath, subfolder);
            Directory.CreateDirectory(directoryPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(directoryPath, fileName);

            // Save file
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Return relative path for database storage
            return Path.Combine(subfolder, fileName).Replace('\\', '/');
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving file {FileName}", file?.FileName);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<(Stream stream, string fileName, string contentType)>> GetFileAsync(string relativePath)
    {
        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), FileStoragePath, relativePath);
            
            if (!File.Exists(fullPath))
                return new ErrorModel(ErrorEnum.FileNotFound);

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var fileName = Path.GetFileName(fullPath);
            var contentType = GetContentType(Path.GetExtension(fileName));

            return (stream, fileName, contentType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving file {FilePath}", relativePath);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public async Task<Result<bool>> DeleteFileAsync(string relativePath)
    {
        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), FileStoragePath, relativePath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting file {FilePath}", relativePath);
            return new ErrorModel(ErrorEnum.InternalServerError);
        }
    }

    public string GenerateEncryptionKey()
    {
        using var rng = RandomNumberGenerator.Create();
        var keyBytes = new byte[32]; // 256-bit key
        rng.GetBytes(keyBytes);
        return Convert.ToBase64String(keyBytes);
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
}