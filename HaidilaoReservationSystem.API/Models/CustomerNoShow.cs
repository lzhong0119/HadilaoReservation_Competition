using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaidilaoReservationSystem.API.Models
{
    [Table("customernoshows")]
    public class CustomerNoShow
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NoShowId { get; set; }

        public int OutletId { get; set; }

        [Required]
        [StringLength(15)]
        public string ContactNumber { get; set; }

        public int NoShowCount { get; set; } = 1;

        [Required]
        public string Status { get; set; } = "Warning";

        public DateTime LastNoShowDate { get; set; } = DateTime.Now;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ExpiredAt { get; set; }

        public string Reason { get; set; } = "No show for reservation.";

        // Navigation property
        public Outlet? Outlet { get; set; }
    }
}