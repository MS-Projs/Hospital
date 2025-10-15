using Domain.Models.API.Requests;
using FluentValidation;

namespace Application.Validators.Auth;

public class UploadProfilePhotoRequestValidator : AbstractValidator<UploadProfilePhotoRequest>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public UploadProfilePhotoRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Valid user ID is required");

        RuleFor(x => x.Photo)
            .NotNull().WithMessage("Photo file is required")
            .Must(file => file.Length > 0).WithMessage("Photo file cannot be empty")
            .Must(file => file.Length <= MaxFileSize).WithMessage($"Photo file size must not exceed {MaxFileSize / 1024 / 1024}MB")
            .Must(file => AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
            .WithMessage($"Only {string.Join(", ", AllowedExtensions)} files are allowed");
    }
}