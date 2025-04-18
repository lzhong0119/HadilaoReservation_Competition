using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaidilaoReservationSystem.API.Models
{
    public class Outlet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OutletId { get; set; }

        [StringLength(100)]
        public string OutletName { get; set; } = string.Empty;

        [StringLength(255)]
        public string Location { get; set; } = string.Empty;

        [StringLength(100)]
        public string OperatingHours { get; set; } = string.Empty;

        public int? Capacity { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<Queue> Queues { get; set; }
        public ICollection<CustomerNoShow> CustomerNoShows { get; set; }
    }
}