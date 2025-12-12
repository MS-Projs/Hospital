using Domain.Models.API.Requests;
using FluentValidation;

namespace Application.Validators.Auth;

public class CreateSessionRequestValidator : AbstractValidator<CreateSessionRequest>
{
    public CreateSessionRequestValidator()
    {
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^(\+?998)\d{9}$") .WithMessage("Invalid phone number format");
    }
}