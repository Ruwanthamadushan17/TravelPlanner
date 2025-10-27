using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TravelPlanner.Domain.Entities;
using TravelPlanner.Infrastructure;
using TravelPlanner.Infrastructure.Persistence.Repositories;
using TravelPlanner.Tests.Integration.Database;

namespace TravelPlanner.Tests.Integration.Repositories
{

    [Collection("db")]
    public class TripRepositoryTests
    {
        private readonly TestDatabase _db;
        public TripRepositoryTests(TestDatabase db) => _db = db;

        private TravelPlannerDb CreateDb() =>
            new(new DbContextOptionsBuilder<TravelPlannerDb>()
                .UseSqlServer(_db.ConnectionString)
                .Options);

        [Fact]
        public async Task AddAsync_Persists_Trip()
        {
            await _db.ResetAsync();

            using var ctx = CreateDb();
            var repo = new TripRepository(ctx);

            var trip = new Trip
            {
                OwnerEmail = "owner@example.com",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 1, 5),
                Budget = 500m,
                Destinations = { new Destination { City = "Paris", Country = "FR" } }
            };

            await repo.AddAsync(trip);
            await ctx.SaveChangesAsync();

            var saved = await ctx.Trips
                .Include(t => t.Destinations)
                .SingleAsync(t => t.Id == trip.Id);

            saved.OwnerEmail.Should().Be("owner@example.com");
            saved.Destinations.Should().ContainSingle(d => d.City == "Paris" && d.Country == "FR");
        }
    }
}