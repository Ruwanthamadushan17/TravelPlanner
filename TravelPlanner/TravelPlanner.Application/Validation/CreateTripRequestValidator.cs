using FluentValidation;
using TravelPlanner.Application.Models;

namespace TravelPlanner.Application.Validation;

public class CreateTripRequestValidator : AbstractValidator<CreateTripRequest>
{
    public CreateTripRequestValidator()
    {
        RuleFor(x => x.OwnerEmail)
            .NotEmpty().WithMessage("OwnerEmail is required.")
            .EmailAddress().WithMessage("OwnerEmail must be a valid email address.");

        RuleFor(x => x.StartDate)
            .NotEqual(default(DateOnly)).WithMessage("StartDate is required.");

        RuleFor(x => x.EndDate)
            .NotEqual(default(DateOnly)).WithMessage("EndDate is required.")
            .Must((model, end) => end > model.StartDate)
            .WithMessage("EndDate must be after StartDate.");

        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0).WithMessage("Budget must be non-negative.");

        RuleFor(x => x.Destinations)
            .NotNull().WithMessage("Destinations is required.")
            .Must(d => d != null && d.Count > 0).WithMessage("At least one destination is required.");

        RuleForEach(x => x.Destinations)
            .SetValidator(new DestinationCreateValidator());
    }
}
