using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaidilaoReservationSystem.API.Models
{
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        [StringLength(15)]
        public string ContactNumber { get; set; } = string.Empty;

        public string? Message { get; set; }

        [StringLength(50)]
        public string SentVia { get; set; } = "Whatsapp";

        [Required]
        [StringLength(10)]
        [Column(TypeName = "varchar(10)")]
        [RegularExpression("Sent|Failed", ErrorMessage = "Invalid status")]
        public string Status { get; set; } = "Sent";

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }
    }
}