using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaidilaoReservationSystem.API.Models
{
    public class BannedCustomer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BanId { get; set; }

        [Required]
        [ForeignKey("Outlet")]
        public int OutletId { get; set; }

        [StringLength(15)]
        public string? ContactNumber { get; set; }

        public string? Reason { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime BannedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ExpiredAt { get; set; }

        // Navigation property
        public Outlet Outlet { get; set; }
    }
}