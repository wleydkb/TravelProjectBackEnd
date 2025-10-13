using Microsoft.EntityFrameworkCore;
using Travel.Core.Entities;

namespace Travel.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<FlightCache> FlightCaches { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.Email).IsUnique();
                b.Property(x => x.FullName).IsRequired().HasMaxLength(200);
                b.Property(x => x.Email).IsRequired().HasMaxLength(200);
            });

            modelBuilder.Entity<Booking>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
                b.HasOne(x => x.User)
                 .WithMany(u => u.Bookings)
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                b.Property(x => x.Status).HasMaxLength(50);
                b.Property(x => x.Currency).HasMaxLength(10);
            });

            modelBuilder.Entity<FlightCache>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.OfferId);
                b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            });
            modelBuilder.Entity<FlightCache>()
       .HasIndex(x => new { x.Origin, x.Destination, x.DepartureDate, x.CachedAt });
        }
    }
}
