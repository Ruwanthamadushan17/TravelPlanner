using FluentAssertions;
using System.Net.Http.Json;
using TravelPlanner.Tests.Integration.Database;

namespace TravelPlanner.Tests.Integration.Api
{

    [Collection("db")]
    public class TripsApiTests
    {
        private readonly TestDatabase _db;
        public TripsApiTests(TestDatabase db) => _db = db;

        [Fact]
        public async Task Post_Then_Get_Returns_Trip()
        {
            await _db.ResetAsync();

            await using var factory = new ApiFactory(_db.ConnectionString);
            var client = factory.CreateClient();

            var create = new
            {
                ownerEmail = "api@example.com",
                startDate = "2025-06-01",
                endDate = "2025-06-03",
                budget = 750,
                destinations = new[] { new { city = "Berlin", country = "DE" } }
            };

            var post = await client.PostAsJsonAsync("/api/v1/Trips", create);
            post.EnsureSuccessStatusCode();

            var list = await client.GetFromJsonAsync<dynamic[]>("/api/v1/trips?ownerEmail=api@example.com");
            list!.Length.Should().Be(1);
        }
    }
}