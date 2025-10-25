using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TravelPlanner.Api.Controllers;
using TravelPlanner.Application.Models;
using TravelPlanner.Application.Services;

namespace TravelPlanner.Api.UnitTests.Controllers;

public class TripsControllerTests
{
    private readonly Mock<ITripService> _service = new(MockBehavior.Strict);

    private TripsController CreateController() => new(_service.Object);

    [Fact]
    public async Task GetById_WhenFound_ReturnsOkWithDto()
    {
        var ct = CancellationToken.None;
        var dto = new TripDto { Id = 123, OwnerEmail = "a@x.com" };

        _service.Setup(s => s.GetByIdAsync(123, ct)).ReturnsAsync(dto);

        var controller = CreateController();

        var result = await controller.GetById(123, ct);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
        ok.Value.Should().BeSameAs(dto);

        _service.Verify(s => s.GetByIdAsync(123, ct), Times.Once);
        _service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetById_WhenMissing_Returns404()
    {
        var ct = CancellationToken.None;

        _service.Setup(s => s.GetByIdAsync(42, ct)).ReturnsAsync((TripDto?)null);

        var controller = CreateController();

        var result = await controller.GetById(42, ct);

        result.Result.Should().BeOfType<NotFoundResult>();

        _service.Verify(s => s.GetByIdAsync(42, ct), Times.Once);
        _service.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("owner@example.com")]
    public async Task List_Always_ReturnsOkWithMappedList(string? ownerEmail)
    {
        var ct = CancellationToken.None;

        IEnumerable<TripDto> expected = new List<TripDto>
        {
            new TripDto { Id = 1, OwnerEmail = "a@x.com" },
            new TripDto { Id = 2, OwnerEmail = "b@x.com" }
        };

        _service.Setup(s => s.ListAsync(ownerEmail, ct)).ReturnsAsync((IReadOnlyList<TripDto>)new List<TripDto>(expected));

        var controller = CreateController();

        var result = await controller.List(ownerEmail, ct);

        var ok = result.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
        ok.Value.Should().BeAssignableTo<IEnumerable<TripDto>>();
        ok.Value.Should().BeEquivalentTo(expected);

        _service.Verify(s => s.ListAsync(ownerEmail, ct), Times.Once);
        _service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Create_WhenValid_Returns201CreatedAtAction_WithBody_AndRouteId()
    {
        var ct = CancellationToken.None;

        var request = MakeCreateRequest();
        var created = new TripDto { Id = 987, OwnerEmail = request.OwnerEmail };

        _service.Setup(s => s.CreateAsync(request, ct)).ReturnsAsync(created);

        var controller = CreateController();

        var result = await controller.Create(request, ct);

        var createdAt = result.Result as CreatedAtActionResult;
        createdAt.Should().NotBeNull();
        createdAt!.StatusCode.Should().Be(201);
        createdAt.ActionName.Should().Be(nameof(TripsController.GetById));
        createdAt.RouteValues.Should().ContainKey("id");
        createdAt.RouteValues!["id"].Should().Be(created.Id);
        createdAt.Value.Should().BeSameAs(created);

        _service.Verify(s => s.CreateAsync(request, ct), Times.Once);
        _service.VerifyNoOtherCalls();
    }

    private static CreateTripRequest MakeCreateRequest() =>
        new CreateTripRequest(
            OwnerEmail: "owner@example.com",
            StartDate: new DateOnly(2025, 1, 1),
            EndDate: new DateOnly(2025, 1, 3),
            Budget: 250m,
            Destinations: new List<DestinationCreate>());
}
