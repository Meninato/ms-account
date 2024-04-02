using Account.Api.Core;
using Account.Data;
using FluentValidation;

namespace Account.Api.V1.Models.Requests.Validations;

public class RegisterAccountRequestValidator : AppAbstractValidator<RegisterAccountRequest>
{
    public RegisterAccountRequestValidator(DataContext context)
    {
        RuleFor(obj => obj.FirstName)
            .NotEmpty().WithMessage("FirstName is required and cannot be empty")
            .MaximumLength(50).WithMessage("FirstName maximum length exceeds.");

        RuleFor(obj => obj.LastName)
            .NotEmpty().WithMessage("LastName is required and cannot be empty")
            .MaximumLength(100).WithMessage("LastName maximum length exceeds.");

        RuleFor(obj => obj.Email)
            .NotEmpty().WithMessage("Email is required and cannot be empty")
            .MaximumLength(254).WithMessage("Email maximum length exceeds.")
            .EmailAddress().WithMessage("Email is not a valid email address");

        RuleFor(obj => obj.Password)
            .NotEmpty().WithMessage("Password is required and cannot be empty.")
            .MinimumLength(8).WithMessage("Password is too short.");

        RuleFor(obj => obj.ConfirmPassword)
            .NotEmpty().WithMessage("ConfirmPassword is required and cannot be empty")
            .Equal(obj => obj.Password).WithMessage("Password doesn't match");

        RuleFor(obj => obj.AcceptTerms)
            .NotEmpty().WithMessage("AcceptTerms is required and cannot be empty")
            .Equal(true).WithMessage("You must accept the terms to create an account.");

        RuleFor(obj => obj.Roles)
            .NotEmpty().WithMessage("Roles is required and cannot be empty")
            .Must(requestedRoles =>
            {
                var toLowerRequestedRoles = requestedRoles.Select(role => role.ToLower()).ToList();
                var foundRoles = context.Roles.Where(r => toLowerRequestedRoles.Contains(r.Name.ToLower())).ToList();
                return foundRoles.Count > 0 && foundRoles.Count == requestedRoles.Count;
            }).WithMessage("Invalid roles");

    }
}