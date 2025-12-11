using Domain.Models.API.Requests;
using FluentValidation;

namespace Application.Validators.Auth;

public class CreateSessionRequestValidator : AbstractValidator<CreateSessionRequest>
{
    public CreateSessionRequestValidator()
    {
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^(\+?998)(33|90|91|94|95|97|99|88|71|73|78)\d{7}$").WithMessage("Invalid phone number format");
    }
}