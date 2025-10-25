using Microsoft.EntityFrameworkCore;
using TravelPlanner.Domain.Entities;
using TravelPlanner.Infrastructure;
using TravelPlanner.Infrastructure.Persistence.Repositories;

namespace TravelPlanner.Tests.Infrastructure;

public class TripRepositoryTests
{
    private static TravelPlannerDb CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<TravelPlannerDb>()
            .UseInMemoryDatabase(databaseName: name)
            .Options;
        return new TravelPlannerDb(options);
    }

    [Fact]
    public async Task AddAsync_Persists_Trip()
    {
        using var db = CreateDb(nameof(AddAsync_Persists_Trip));
        var repo = new TripRepository(db);

        var trip = MakeTrip("x@x.com", 2025, 1, 1, 10, Dest("Rome", "IT"));
        await repo.AddAsync(trip);
        await db.SaveChangesAsync();

        Assert.True(trip.Id > 0);
        var roundtrip = await db.Trips.Include(t => t.Destinations).FirstOrDefaultAsync(t => t.Id == trip.Id);
        Assert.NotNull(roundtrip);
        Assert.Single(roundtrip!.Destinations);
    }

    [Fact]
    public async Task AddAsync_Does_Not_Save_Without_SaveChanges()
    {
        using var db = CreateDb(nameof(AddAsync_Does_Not_Save_Without_SaveChanges));
        var repo = new TripRepository(db);

        await repo.AddAsync(MakeTrip("x@x.com", 2025, 1, 1), CancellationToken.None);

        Assert.Equal(0, await db.Trips.CountAsync(CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Including_Destinations()
    {
        using var db = CreateDb(nameof(GetByIdAsync_Returns_Including_Destinations));
        var repo = new TripRepository(db);

        var trip = MakeTrip("x@x.com", 2025, 2, 1, 100m, Dest("Rome", "IT"));
        await db.Trips.AddAsync(trip);
        await db.SaveChangesAsync();

        var found = await repo.GetByIdAsync(trip.Id);
        Assert.NotNull(found);
        Assert.Single(found!.Destinations);
        Assert.Equal("Rome", found.Destinations[0].City);
    }

    [Fact]
    public async Task ListAsync_Filters_By_Owner_And_Orders_By_StartDate()
    {
        using var db = CreateDb(nameof(ListAsync_Filters_By_Owner_And_Orders_By_StartDate));
        var repo = new TripRepository(db);

        var t1 = MakeTrip("a@x.com", 2025, 1, 10);
        var t2 = MakeTrip("a@x.com", 2025, 1, 1);
        var t3 = MakeTrip("b@x.com", 2025, 1, 5);
        await db.Trips.AddRangeAsync(t1, t2, t3);
        await db.SaveChangesAsync();

        var result = await repo.ListAsync("a@x.com");

        Assert.Equal(2, result.Count);
        Assert.Collection(result,
            first => Assert.Equal(new DateOnly(2025, 1, 1), first.StartDate),
            second => Assert.Equal(new DateOnly(2025, 1, 10), second.StartDate)
        );
    }

    [Fact]
    public async Task ListAsync_NoOwner_Returns_All_Ordered_And_NotTracked()
    {
        using var db = CreateDb(nameof(ListAsync_NoOwner_Returns_All_Ordered_And_NotTracked));
        var repo = new TripRepository(db);

        var t1 = MakeTrip("a@x.com", 2025, 3, 1);
        var t2 = MakeTrip("b@x.com", 2025, 1, 1);
        var t3 = MakeTrip("c@x.com", 2025, 2, 1, 30m, Dest("Paris", "FR"));
        await db.Trips.AddRangeAsync(t1, t2, t3);
        await db.SaveChangesAsync();

        var result = await repo.ListAsync(null, CancellationToken.None);

        Assert.Equal(new[] { t2.Id, t3.Id, t1.Id }, result.Select(r => r.Id).ToArray());

        foreach (var e in result)
            Assert.Equal(EntityState.Detached, db.Entry(e).State);
    }

    [Fact]
    public async Task Remove_Deletes_After_SaveChanges()
    {
        using var db = CreateDb(nameof(Remove_Deletes_After_SaveChanges));
        var repo = new TripRepository(db);

        var trip = MakeTrip("del@x.com", 2025, 1, 1);
        await db.Trips.AddAsync(trip);
        await db.SaveChangesAsync();

        repo.Remove(trip);
        Assert.Equal(EntityState.Deleted, db.Entry(trip).State);

        await db.SaveChangesAsync();

        Assert.False(await db.Trips.AnyAsync(t => t.Id == trip.Id));
    }

    private static Trip MakeTrip(string owner, int y, int m, int d, decimal budget = 100m, params Destination[] dests)
    {
        var trip = new Trip
        {
            OwnerEmail = owner,
            StartDate = new DateOnly(y, m, d),
            EndDate = new DateOnly(y, m, d).AddDays(1),
            Budget = budget
        };
        foreach (var d0 in dests)
        {
            trip.Destinations.Add(d0);
        }
        return trip;
    }

    private static Destination Dest(string city, string country) => new() { City = city, Country = country };

}

