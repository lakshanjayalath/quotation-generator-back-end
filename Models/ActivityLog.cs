using System;
using System.ComponentModel.DataAnnotations;

namespace quotation_generator_back_end.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = string.Empty;

        public int RecordId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty; // Create, Update, Delete

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? PerformedBy { get; set; }

        public DateTime Timestamp { get; set; }

        // Foreign key to User (optional)
        public int? UserId { get; set; }

        // Navigation property
        public User? User { get; set; }
    }
}
