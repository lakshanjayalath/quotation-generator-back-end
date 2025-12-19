using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using quotation_generator_back_end.Data;
using quotation_generator_back_end.DTOs.Auth;
using quotation_generator_back_end.Helpers;
using quotation_generator_back_end.Models;
using quotation_generator_back_end.Services;
using System.Security.Claims;

namespace quotation_generator_back_end.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IActivityLogger _activityLogger;

        public AuthController(ApplicationDbContext context, IJwtService jwtService, IActivityLogger activityLogger)
        {
            _context = context;
            _jwtService = jwtService;
            _activityLogger = activityLogger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            // Validate terms acceptance
            if (!request.Terms)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "You must accept the Terms of Use & Privacy Policy"
                });
            }

            // Check if email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

            if (existingUser != null)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "An account with this email already exists"
                });
            }

            // Create new user
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email.ToLower(),
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                Role = "User",  // Default role for new registrations
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                // Default settings
                TwoFactorAuth = false,
                LoginNotification = true,
                TaskAssignNotification = true,
                DisableRecurringPaymentNotification = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _activityLogger.LogAsync("User", user.Id, "Create", $"User registered: {user.Email}");

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                User = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Role = user.Role ?? "User"
                }
            });
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

            if (user == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            // Verify password
            if (string.IsNullOrEmpty(user.PasswordHash) || 
                !PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }

            // Generate JWT token (longer expiry if rememberMe)
            var token = _jwtService.GenerateToken(user, request.RememberMe);
            var expirationDays = request.RememberMe ? 30 : 1;

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
                User = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Role = user.Role ?? "User"
                }
            });
        }

        /// <summary>
        /// Validate token and get current user info
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<AuthResponseDto>> GetCurrentUser()
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired token"
                });
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "User retrieved successfully",
                User = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    Email = user.Email ?? string.Empty
                }
            });
        }

        /// <summary>
        /// Register a new user by an Admin
        /// </summary>
        [HttpPost("register-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminRegisterResponseDto>> RegisterByAdmin([FromBody] RegisterAdminDto request)
        {
            // Check if email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

            if (existingUser != null)
            {
                return BadRequest(new AdminRegisterResponseDto
                {
                    Success = false,
                    Message = "An account with this email already exists"
                });
            }

            // Validate role
            if (request.Role != "User" && request.Role != "Admin")
            {
                return BadRequest(new AdminRegisterResponseDto
                {
                    Success = false,
                    Message = "Role must be either 'User' or 'Admin'"
                });
            }

            // Get the admin who is creating this user
            var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown Admin";

            // Create new user
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email.ToLower(),
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                Role = request.Role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                // Default settings
                TwoFactorAuth = false,
                LoginNotification = true,
                TaskAssignNotification = true,
                DisableRecurringPaymentNotification = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _activityLogger.LogAsync(
                "User", 
                user.Id, 
                "Created", 
                $"User '{user.Email}' with role '{user.Role}' created by admin '{adminEmail}'"
            );

            return StatusCode(201, new AdminRegisterResponseDto
            {
                Success = true,
                Message = "User registered successfully",
                User = new AdminCreatedUserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    Role = user.Role
                }
            });
        }
    }
}
