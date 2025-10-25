using AutoMapper;
using FluentAssertions;
using Moq;
using TravelPlanner.Application.Abstractions;
using TravelPlanner.Application.Models;
using TravelPlanner.Application.Services;
using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Application.UnitTests.Services;

public sealed class TripServiceTests
{
    private readonly Mock<ITripRepository> _repo = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _uow = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);

    private TripService CreateTripService() => new(_repo.Object, _uow.Object, _mapper.Object);

    [Fact]
    public async Task GetByIdAsync_WhenTripExists_ReturnsMappedDto()
    {
        // Arrange
        var ct = CancellationToken.None;
        var entity = MakeTrip("owner@example.com");
        var expectedDto = MakeTripDto("owner@example.com");

        _repo.Setup(r => r.GetByIdAsync(42, ct)).ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<TripDto>(entity)).Returns(expectedDto);

        var tripService = CreateTripService();

        // Act
        var result = await tripService.GetByIdAsync(42, ct);

        // Assert
        result.Should().BeSameAs(expectedDto);
        _repo.Verify(r => r.GetByIdAsync(42, ct), Times.Once);
        _mapper.Verify(m => m.Map<TripDto>(entity), Times.Once);
        _repo.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
        _mapper.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetByIdAsync_WhenTripMissing_ReturnsNull_AndDoesNotMap()
    {
        // Arrange
        var ct = CancellationToken.None;
        _repo.Setup(r => r.GetByIdAsync(7, ct)).ReturnsAsync((Trip?)null);

        var tripService = CreateTripService();

        // Act
        var result = await tripService.GetByIdAsync(7, ct);

        // Assert
        result.Should().BeNull();
        _repo.Verify(r => r.GetByIdAsync(7, ct), Times.Once);
        _mapper.Verify(m => m.Map<TripDto>(It.IsAny<Trip>()), Times.Never);
        _repo.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
        _mapper.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ListAsync_NoOwner_ReturnsMappedList()
    {
        // Arrange
        var ct = CancellationToken.None;

        IReadOnlyList<Trip> entities = new List<Trip>
        {
            MakeTrip("a@example.com"),
            MakeTrip("b@example.com")
        };

        IReadOnlyList<TripDto> expected = new List<TripDto>
        {
            MakeTripDto("a@example.com"),
            MakeTripDto("b@example.com")
        };

        _repo.Setup(r => r.ListAsync(null, ct)).ReturnsAsync(entities);
        _mapper.Setup(m => m.Map<IReadOnlyList<TripDto>>(entities)).Returns(expected);

        var tripService = CreateTripService();

        // Act
        var result = await tripService.ListAsync(null, ct);

        // Assert
        result.Should().BeSameAs(expected);
        _repo.Verify(r => r.ListAsync(null, ct), Times.Once);
        _mapper.Verify(m => m.Map<IReadOnlyList<TripDto>>(entities), Times.Once);
        _repo.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
        _mapper.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ListAsync_WithOwner_ForwardsOwner_AndToken()
    {
        // Arrange
        var ct = CancellationToken.None;
        const string owner = "owner@example.com";

        IReadOnlyList<Trip> entities = new List<Trip> { MakeTrip(owner) };
        IReadOnlyList<TripDto> expected = new List<TripDto> { MakeTripDto(owner) };

        _repo.Setup(r => r.ListAsync(owner, ct)).ReturnsAsync(entities);
        _mapper.Setup(m => m.Map<IReadOnlyList<TripDto>>(entities)).Returns(expected);

        var tripService = CreateTripService();

        // Act
        var result = await tripService.ListAsync(owner, ct);

        // Assert
        result.Should().BeSameAs(expected);
        _repo.Verify(r => r.ListAsync(owner, ct), Times.Once);
        _mapper.Verify(m => m.Map<IReadOnlyList<TripDto>>(entities), Times.Once);
        _repo.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
        _mapper.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ListAsync_WhenRepoReturnsEmpty_ReturnsEmptyMappedList(string? owner)
    {
        var ct = CancellationToken.None;
        IReadOnlyList<Trip> entities = new List<Trip>();
        IReadOnlyList<TripDto> mapped = new List<TripDto>();

        _repo.Setup(r => r.ListAsync(owner, ct)).ReturnsAsync(entities);
        _mapper.Setup(m => m.Map<IReadOnlyList<TripDto>>(entities)).Returns(mapped);

        var tripService = CreateTripService();
        var result = await tripService.ListAsync(owner, ct);

        result.Should().BeSameAs(mapped);
    }

    [Fact]
    public async Task CreateAsync_Maps_Adds_Saves_MapsBack_InOrder()
    {
        // Arrange
        var ct = CancellationToken.None;

        var request = MakeCreateRequest();
        var entity = MakeTrip(request.OwnerEmail);
        var expectedDto = MakeTripDto(request.OwnerEmail);

        _mapper.Setup(m => m.Map<Trip>(request)).Returns(entity);

        var sequence = new MockSequence();
        _repo.InSequence(sequence)
             .Setup(r => r.AddAsync(entity, ct))
             .Returns(Task.CompletedTask);
        _uow.InSequence(sequence)
            .Setup(u => u.SaveChangesAsync(ct))
            .ReturnsAsync(0);

        _mapper.Setup(m => m.Map<TripDto>(entity)).Returns(expectedDto);

        var tripService = CreateTripService();

        // Act
        var result = await tripService.CreateAsync(request, ct);

        // Assert
        result.Should().BeSameAs(expectedDto);

        _mapper.Verify(m => m.Map<Trip>(request), Times.Once);
        _repo.Verify(r => r.AddAsync(entity, ct), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(ct), Times.Once);
        _mapper.Verify(m => m.Map<TripDto>(entity), Times.Once);

        _repo.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
        _mapper.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_WhenAddFails_DoesNotSave_Throws()
    {
        var ct = CancellationToken.None;
        var request = MakeCreateRequest();
        var entity = MakeTrip(request.OwnerEmail);

        _mapper.Setup(m => m.Map<Trip>(request)).Returns(entity);
        _repo.Setup(r => r.AddAsync(entity, ct)).ThrowsAsync(new InvalidOperationException("boom"));

        var tripService = CreateTripService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => tripService.CreateAsync(request, ct));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Trip MakeTrip(string owner) =>
        new Trip
        {
            OwnerEmail = owner,
        };

    private static TripDto MakeTripDto(string owner) =>
        new TripDto
        {
            OwnerEmail = owner
        };

    private static CreateTripRequest MakeCreateRequest() =>
        new CreateTripRequest(
            OwnerEmail: "owner@example.com",
            StartDate: new DateOnly(2025, 1, 1),
            EndDate: new DateOnly(2025, 1, 3),
            Budget: 250m,
            Destinations: new List<DestinationCreate>());
}
