using FluentValidation.TestHelper;
using TravelPlanner.Application.Models;
using TravelPlanner.Application.Validation;

namespace TravelPlanner.Application.UnitTests.Validation;

public class DestinationCreateValidatorTests
{
    private readonly DestinationCreateValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void City_WhenMissing_IsInvalid(string? value)
    {
        var model = ValidDestination() with { City = value! };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.City)
              .WithErrorMessage("City is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Country_WhenMissing_IsInvalid(string? value)
    {
        var model = ValidDestination() with { Country = value! };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Country)
              .WithErrorMessage("Country is required.");
    }

    [Fact]
    public void ValidDestination_IsValid()
    {
        var model = ValidDestination();

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MissingCityAndCountry_ReportsBothErrors()
    {
        var model = ValidDestination() with { City = "", Country = "" };

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.City);
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }

    private static DestinationCreate ValidDestination() =>
        new DestinationCreate("London", "UK");
}
