using Microsoft.EntityFrameworkCore;
using HaidilaoReservationSystem.API.Models;

namespace HaidilaoReservationSystem.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Outlet> Outlets { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Queue> Queues { get; set; }
        public DbSet<CustomerNoShow> CustomerNoShows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerNoShow>(entity =>
            {
                entity.ToTable("customernoshows");

                entity.Property(e => e.ExpiredAt)
                    .HasComputedColumnSql(
                        "CASE " +
                        "WHEN Status = 'Warning' THEN DATE_ADD(LastNoShowDate, INTERVAL 30 DAY) " +
                        "WHEN Status = 'Suspended' THEN DATE_ADD(LastNoShowDate, INTERVAL 7 DAY) " +
                        "WHEN Status = 'Banned' THEN DATE_ADD(LastNoShowDate, INTERVAL 30 DAY) " +
                        "END"
                    );

                entity.HasOne(e => e.Outlet)
                    .WithMany(o => o.CustomerNoShows)
                    .HasForeignKey(e => e.OutletId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Outlet>(entity =>
            {
                entity.ToTable("outlets");
            });
        }
    }
}