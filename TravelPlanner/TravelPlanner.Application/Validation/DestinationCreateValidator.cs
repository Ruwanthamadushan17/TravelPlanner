using FluentValidation;
using TravelPlanner.Application.Models;

namespace TravelPlanner.Application.Validation;

public class DestinationCreateValidator : AbstractValidator<DestinationCreate>
{
    public DestinationCreateValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.");
        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.");
    }
}
