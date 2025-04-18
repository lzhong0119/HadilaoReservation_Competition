using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HaidilaoReservationSystem.API.Models
{
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReservationId { get; set; }

        public int OutletId { get; set; }

        [StringLength(50)]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(15)]
        public string ContactNumber { get; set; } = string.Empty;

        public int? NumberOfGuest { get; set; }

        [Required]
        public DateTime ReservationDateTime { get; set; }

        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        [RegularExpression("Pending|Confirmed|Cancelled|Completed", ErrorMessage = "Invalid status")]
        public string Status { get; set; } = "Pending";

        public string? SpecialRequest { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        [ValidateNever]
        [JsonIgnore]
        public Outlet? Outlet { get; set; } // Make nullable
    }
}