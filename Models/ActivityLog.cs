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
        public string ActionType { get; set; } = string.Empty; // e.g., Create, Update, Delete

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string PerformedBy { get; set; } = "System";

        [MaxLength(50)]
        public string? PerformedByRole { get; set; }

        [MaxLength(200)]
        public string? PerformedByEmail { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Optional FK to User
        public int? UserId { get; set; }

        // Navigation property
        public User? User { get; set; }
    }
}
