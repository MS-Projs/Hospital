using Domain.Models.API.Requests;
using FluentValidation;

namespace Application.Validators;

public class SignUpRequestValidator : AbstractValidator<SignUpRequest>
{
    public SignUpRequestValidator()
    {
        RuleFor(s => s.Phone)
            .NotEmpty()
            .WithMessage("Phone is required")
            .Matches(@"^\d{12}$")
            .WithMessage("Invalid phone format");

        RuleFor(s => s.FirstName)
            .NotEmpty()
            .WithMessage("First name is required");

        RuleFor(s => s.LastName)
            .NotEmpty()
            .WithMessage("Last name is required");
    }
}