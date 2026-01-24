using FluentValidation;
using ServiceHubPro.Application.Features.Services.Commands;

namespace ServiceHubPro.Application.Features.Services.Validators;

public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Service name is required")
            .MaximumLength(200).WithMessage("Service name must not exceed 200 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0");
    }
}
