using Account.Api.Core;
using FluentValidation;

namespace Account.Api.V1.Models.Requests.Validations;

public class AuthenticateRequestValidator : AppAbstractValidator<AuthenticateRequest>
{
    public AuthenticateRequestValidator()
    {
        RuleFor(obj => obj.Email)
            .NotEmpty().WithMessage("Email is required and cannot be empty.")
            .MaximumLength(254).WithMessage("Email maximum length exceeds.")
            .EmailAddress().WithMessage("Email is not a valid email address");

        RuleFor(obj => obj.Password)
            .NotEmpty().WithMessage("Password is required and cannot be empty.");
    }
}