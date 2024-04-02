using FluentValidation.Results;
using FluentValidation;

namespace Account.Api.Core;

public class AppAbstractValidator<T> : AbstractValidator<T>
{
    public AppAbstractValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
    }

    protected override bool PreValidate(ValidationContext<T> context, ValidationResult result)
    {
        if (context.InstanceToValidate == null)
        {
            result.Errors.Add(new ValidationFailure("", "Please ensure a model was supplied."));
            return false;
        }
        return true;
    }
}