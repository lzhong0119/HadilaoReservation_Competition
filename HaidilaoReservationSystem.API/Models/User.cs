using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaidilaoReservationSystem.API.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment in DB
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty; // Match NULL DEFAULT ''

        [Required]
        [StringLength(15)]
        public string ContactNumber { get; set; } = string.Empty;

        [StringLength(100)] // Remove [Required] to match NULL DEFAULT
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        // ENUM handling (Option 1 - String matching DB ENUM)
        [Required]
        [StringLength(10)]
        [Column(TypeName = "varchar(10)")] // Force varchar storage
        [RegularExpression("Staff|Admin", ErrorMessage = "Role must be 'Staff' or 'Admin'")]
        public string Role { get; set; } = "Staff"; // Match DB ENUM values and default

        [ForeignKey("Outlet")]
        public int? OutletId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; } // DB handles default

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; } // DB handles auto-update
    }   
}
