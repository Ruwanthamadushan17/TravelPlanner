using FluentValidation.TestHelper;
using TravelPlanner.Application.Models;
using TravelPlanner.Application.Validation;

namespace TravelPlanner.Application.UnitTests.Validation;

public class CreateTripRequestValidatorTests
{
    private readonly CreateTripRequestValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void OwnerEmail_WhenMissing_IsInvalid(string? email)
    {
        var model = ValidRequest() with { OwnerEmail = email! };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.OwnerEmail)
              .WithErrorMessage("OwnerEmail is required.");
    }

    [Theory]
    [InlineData("invalidemail")]
    [InlineData("john@")]
    [InlineData("@example.com")]
    public void OwnerEmail_WhenNotEmail_IsInvalid(string badEmail)
    {
        var model = ValidRequest() with { OwnerEmail = badEmail };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.OwnerEmail)
              .WithErrorMessage("OwnerEmail must be a valid email address.");
    }

    [Fact]
    public void StartDate_WhenDefault_IsInvalid()
    {
        var model = ValidRequest() with { StartDate = default };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.StartDate)
              .WithErrorMessage("StartDate is required.");
    }

    [Fact]
    public void EndDate_WhenDefault_IsInvalid()
    {
        var model = ValidRequest() with { EndDate = default };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.EndDate)
              .WithErrorMessage("EndDate is required.");
    }

    [Theory]
    [InlineData(2025, 1, 2, 2025, 1, 1)]
    [InlineData(2025, 1, 1, 2025, 1, 1)]
    public void EndDate_WhenNotAfterStart_IsInvalid(
        int sy, int sm, int sd, int ey, int em, int ed)
    {
        var start = new DateOnly(sy, sm, sd);
        var end = new DateOnly(ey, em, ed);

        var model = ValidRequest() with { StartDate = start, EndDate = end };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.EndDate)
              .WithErrorMessage("EndDate must be after StartDate.");
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Budget_WhenNegative_IsInvalid(double bad)
    {
        var model = ValidRequest() with { Budget = (decimal)bad };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Budget)
              .WithErrorMessage("Budget must be non-negative.");
    }

    [Fact]
    public void Destinations_WhenNull_IsInvalid()
    {
        var model = ValidRequest() with { Destinations = null! };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Destinations)
              .WithErrorMessage("Destinations is required.");
    }

    [Fact]
    public void Destinations_WhenEmpty_IsInvalid()
    {
        var model = ValidRequest() with { Destinations = new List<DestinationCreate>() };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Destinations)
              .WithErrorMessage("At least one destination is required.");
    }

    [Fact]
    public void Destinations_WithItem_Passes_CountRule()
    {
        var model = ValidRequest() with
        {
            Destinations = new List<DestinationCreate> { ValidDestination() }
        };

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Destinations);
    }

    private static DestinationCreate ValidDestination() =>
        new DestinationCreate("London", "UK");

    private static CreateTripRequest ValidRequest() =>
        new CreateTripRequest(
            OwnerEmail: "owner@example.com",
            StartDate: new DateOnly(2025, 1, 1),
            EndDate: new DateOnly(2025, 1, 3),
            Budget: 100m,
            Destinations: new List<DestinationCreate> { ValidDestination() }
        );
}
