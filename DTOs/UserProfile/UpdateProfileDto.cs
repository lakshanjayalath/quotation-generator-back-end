using System.ComponentModel.DataAnnotations;

namespace quotation_generator_back_end.DTOs.UserProfile
{
    /// <summary>
    /// DTO for updating user profile information
    /// </summary>
    public class UpdateProfileDto
    {
        // Basic Information
        [MaxLength(200)]
        public string? FirstName { get; set; }

        [MaxLength(200)]
        public string? LastName { get; set; }

        [MaxLength(200)]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        // Address
        [MaxLength(500)]
        public string? Street { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        // Preferences
        [MaxLength(50)]
        public string? Language { get; set; }

        [MaxLength(50)]
        public string? PreferredContactMethod { get; set; }

        public bool Notifications { get; set; } = true;
    }
}
