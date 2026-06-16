
using CareOS.Api.DTOs;
using CareOS.Api.Helpers;
using CareOS.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareOS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Validation failed", errors });
            }

            // Always use uppercase for role to ensure consistency
            request.Role = (request.Role ?? string.Empty).ToUpperInvariant();

            if (!ValidationHelper.IsValidEmail(request.Email) && request.Role == "CARETAKER")
            {
                return BadRequest(new { message = "Invalid email format" });
            }

            if (!ValidationHelper.IsValidPhoneNumber(request.PhoneNumber))
            {
                return BadRequest(new { message = "Invalid phone number format. Use format: +92-3XX-XXXXXXX" });
            }

            var passwordValidation = ValidationHelper.ValidatePassword(request.Password);
            if (!passwordValidation.isValid)
            {
                return BadRequest(new { message = passwordValidation.message });
            }

            // Validate role
            if (request.Role != "ELDER" && request.Role != "CARETAKER")
            {
                return BadRequest(new { message = "Role must be either ELDER or CARETAKER" });
            }

            // Validate PIN for ELDER
            if (request.Role == "ELDER")
            {
                if (string.IsNullOrEmpty(request.Pin))
                {
                    return BadRequest(new { message = "PIN is required for Elder registration" });
                }

                if (!ValidationHelper.IsValidPin(request.Pin))
                {
                    return BadRequest(new { message = "PIN must be exactly 4 digits" });
                }
            }

            // Validate Email/Password for CARETAKER
            if (request.Role == "CARETAKER" && (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password)))
            {
                return BadRequest(new { message = "Email and Password are required for Caretaker registration" });
            }

            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Always use uppercase for role to ensure consistency
            request.Role = (request.Role ?? string.Empty).ToUpperInvariant();

            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        // GET: api/Auth/me (Get current logged-in user)
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _authService.GetUserByIdAsync(userId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        // GET: api/Auth/user/{id}
        [Authorize]
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var result = await _authService.GetUserByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        // POST: api/Auth/forgot-password
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            var result = await _authService.ForgotPasswordAsync(request.Email);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Auth/verify-reset-code
        [HttpPost("verify-reset-code")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeDto request)
        {
            var result = await _authService.VerifyResetCodeAsync(request.Email, request.ResetCode);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Auth/reset-password
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            var result = await _authService.ResetPasswordAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // POST: api/Auth/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _authService.ChangePasswordAsync(userId, request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // PUT: api/Auth/update-profile
        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _authService.UpdateProfileAsync(userId, request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }

}