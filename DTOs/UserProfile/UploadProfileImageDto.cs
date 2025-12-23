using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace quotation_generator_back_end.DTOs.UserProfile
{
    /// <summary>
    /// DTO for updating profile image URL (from Supabase) - accepts both JSON and FormData
    /// </summary>
    public class UpdateProfileImageDto
    {
        [Required(ErrorMessage = "Image URL is required")]
        [Url(ErrorMessage = "Invalid URL format")]
        [FromForm(Name = "ProfileImageUrl")]
        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO for profile image update
    /// </summary>
    public class ProfileImageResponseDto
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }
    }
}
