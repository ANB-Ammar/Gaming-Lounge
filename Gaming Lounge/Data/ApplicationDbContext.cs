using Gaming_Lounge.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gaming_Lounge.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    public DbSet<GamingCategory> GamingCategories => Set<GamingCategory>();
    public DbSet<GamingStation> GamingStations => Set<GamingStation>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(user =>
        {
            user.Property(x => x.DisplayName).HasMaxLength(80).IsRequired();
        });

        builder.Entity<GamingCategory>(category =>
        {
            category.HasIndex(x => x.Slug).IsUnique();
            category.Property(x => x.Name).HasMaxLength(60).IsRequired();
            category.Property(x => x.Slug).HasMaxLength(80).IsRequired();
            category.Property(x => x.AccentColor).HasMaxLength(20).IsRequired();
        });

        builder.Entity<GamingStation>(station =>
        {
            station.HasIndex(x => x.Slug).IsUnique();
            station.HasIndex(x => new { x.GamingCategoryId, x.Status });
            station.Property(x => x.HourlyRate).HasPrecision(10, 2);
            station.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            station.Property(x => x.RowVersion).IsRowVersion();
            station.HasOne(x => x.GamingCategory)
                .WithMany(x => x.Stations)
                .HasForeignKey(x => x.GamingCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Reservation>(reservation =>
        {
            reservation.ToTable("Reservations", table =>
            {
                table.HasCheckConstraint("CK_Reservations_DurationHours", "[DurationHours] BETWEEN 1 AND 12");
                table.HasCheckConstraint("CK_Reservations_Prices", "[HourlyRate] >= 0 AND [TotalPrice] >= 0");
                table.HasCheckConstraint("CK_Reservations_TimeRange", "[EndTime] > [StartTime]");
            });
            reservation.HasIndex(x => new { x.GamingStationId, x.ReservationDate, x.Status });
            reservation.HasIndex(x => new { x.UserId, x.ReservationDate });
            reservation.Property(x => x.HourlyRate).HasPrecision(10, 2);
            reservation.Property(x => x.TotalPrice).HasPrecision(10, 2);
            reservation.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            reservation.Property(x => x.RowVersion).IsRowVersion();
            reservation.HasOne(x => x.GamingStation)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.GamingStationId)
                .OnDelete(DeleteBehavior.Restrict);
            reservation.HasOne(x => x.User)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
