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
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<BannedCustomer> BannedCustomers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BannedCustomer>()
                .Property(b => b.ExpiredAt)
                .HasComputedColumnSql("DATE_ADD(BannedAt, INTERVAL 7 DAY)");
        }
    }
}
