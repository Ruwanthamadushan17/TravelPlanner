using Microsoft.EntityFrameworkCore;
using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Infrastructure
{
    public class TravelPlannerDb : DbContext
    {
        public TravelPlannerDb(DbContextOptions<TravelPlannerDb> options) : base(options) { }

        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<Destination> Destinations => Set<Destination>();

        public DbSet<Itinerary> Itineraries => Set<Itinerary>();
        public DbSet<ItineraryStep> ItinerarySteps => Set<ItineraryStep>();
        public DbSet<ArrivalStep> ArrivalSteps => Set<ArrivalStep>();
        public DbSet<DayStep> DaySteps => Set<DayStep>();
        public DbSet<DaySuggestion> DaySuggestions => Set<DaySuggestion>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Trip>(e =>
            {
                e.Property(x => x.OwnerEmail).IsRequired().HasMaxLength(256);
                e.Property(x => x.StartDate).IsRequired();
                e.Property(x => x.EndDate).IsRequired();
                e.Property(x => x.Budget).HasColumnType("decimal(18,2)");
                e.HasMany(x => x.Destinations)
                    .WithOne(x => x.Trip)
                    .HasForeignKey(x => x.TripId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
                e.ToTable(t => t.HasCheckConstraint("CK_Trip_Dates", "[EndDate] > [StartDate]"));
            });

            builder.Entity<Destination>(e =>
            {
                e.Property(x => x.City).IsRequired().HasMaxLength(128);
                e.Property(x => x.Country).IsRequired().HasMaxLength(128);
            });

            builder.Entity<Itinerary>(e =>
            {
                e.Property(x => x.TripId).IsRequired();
                e.Property(x => x.TotalCost).HasColumnType("decimal(18,2)");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
                e.HasMany(x => x.Steps)
                    .WithOne(s => s.Itinerary)
                    .HasForeignKey(s => s.ItineraryId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Trip)
                    .WithMany()
                    .HasForeignKey(x => x.TripId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
                e.ToTable(t => t.HasCheckConstraint("CK_Itinerary_TotalCost", "[TotalCost] >= 0"));
            });

            builder.Entity<ItineraryStep>(e =>
            {
                e.Property(s => s.ItineraryId).IsRequired();
                e.Property(s => s.OrderWithInPlan).IsRequired();
                e.HasIndex(s => new { s.ItineraryId, s.OrderWithInPlan }).IsUnique();
                e.ToTable(t => t.HasCheckConstraint("CK_ItineraryStep_Order", "[OrderWithInPlan] >= 0"));
            });

            builder.Entity<ArrivalStep>(e =>
            {
                e.Property(x => x.Notes).HasMaxLength(512);
            });

            builder.Entity<DayStep>(e =>
            {
                e.Property(x => x.Theme).HasMaxLength(256);
                e.Property(x => x.Day).IsRequired();
                e.HasMany(d => d.Suggestions)
                    .WithOne(s => s.DayStep)
                    .HasForeignKey(s => s.DayStepId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
                e.ToTable(t => t.HasCheckConstraint("CK_DayStep_Day", "[Day] >= 1"));
            });

            builder.Entity<DaySuggestion>(e =>
            {
                e.Property(x => x.Text).IsRequired().HasMaxLength(256);
            });
        }
    }
}
