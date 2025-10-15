using Domain.Models.API.Requests;
using FluentValidation;

namespace Application.Validators.Auth;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Valid user ID is required");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("CurrentPassword is required")
            .MinimumLength(6).WithMessage("CurrentPassword must be at least 6 characters long")
            .MaximumLength(100).WithMessage("CurrentPassword must not exceed 100 characters")
            .Matches(@"[0-9]").WithMessage("CurrentPassword must contain at least one digit")
            .Matches(@"[a-z]").WithMessage("CurrentPassword must contain at least one lowercase letter")
            .Matches(@"[A-Z]").WithMessage("CurrentPassword must contain at least one uppercase letter");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("NewPassword is required")
            .MinimumLength(6).WithMessage("NewPassword must be at least 6 characters long")
            .MaximumLength(100).WithMessage("NewPassword must not exceed 100 characters")
            .Matches(@"[0-9]").WithMessage("NewPassword must contain at least one digit")
            .Matches(@"[a-z]").WithMessage("NewPassword must contain at least one lowercase letter")
            .Matches(@"[A-Z]").WithMessage("NewPassword must contain at least one uppercase letter");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}