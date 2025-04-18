    using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

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

            [StringLength(50)]
            public string CustomerName { get; set; } = string.Empty;

            [StringLength(15)]
            public string ContactNumber { get; set; } = string.Empty;

            public int QueuePosition { get; set; }

            public int NumberOfGuest { get; set; }

            public string? SpecialRequest { get; set; }

            [StringLength(20)]
            [Column(TypeName = "varchar(20)")]
            [RegularExpression("Waiting|Called|Completed|Cancelled", ErrorMessage = "Invalid status")]
            public string Status { get; set; } = "Waiting";

            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public DateTime CreatedAt { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public DateTime UpdatedAt { get; set; }

            // Navigation property
            [ValidateNever]
            [JsonIgnore]
            public Outlet? Outlet { get; set; }
        }
    }