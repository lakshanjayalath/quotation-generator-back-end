using System.ComponentModel.DataAnnotations;

namespace quotation_generator_back_end.DTOs.UserProfile
{
    /// <summary>
    /// DTO for updating user notes
    /// </summary>
    public class UpdateNotesDto
    {
        [MaxLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
        public string? Notes { get; set; }
    }
}
