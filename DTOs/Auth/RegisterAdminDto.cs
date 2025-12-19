using System.ComponentModel.DataAnnotations;

namespace quotation_generator_back_end.DTOs.Auth
{
    public class RegisterAdminDto
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(200)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(200)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(User|Admin)$", ErrorMessage = "Role must be either 'User' or 'Admin'")]
        public string Role { get; set; } = "User";
    }

    public class AdminCreatedUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class AdminRegisterResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AdminCreatedUserDto? User { get; set; }
    }
}
