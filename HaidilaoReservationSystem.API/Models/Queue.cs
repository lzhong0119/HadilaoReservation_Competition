using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaidilaoReservationSystem.API.Models
{
    public class Queue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QueueId { get; set; }

        [Required]
        [ForeignKey("Outlet")]
        public int OutletId { get; set; }

        [StringLength(15)]
        public string ContactNumber { get; set; } = string.Empty;

        public int? QueuePosition { get; set; }

        public int? NumberOfGuest { get; set; }

        public string? SpecialRequest { get; set; }

        [Required]
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        [RegularExpression("Waiting|Called|Completed|Cancelled", ErrorMessage = "Invalid status")]
        public string Status { get; set; } = "Waiting";

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public Outlet Outlet { get; set; }
    }
}