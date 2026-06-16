using CareOS.Api.Data;
using CareOS.Api.DTOs;
using CareOS.Api.Helpers;
using CareOS.Api.Models;
using MongoDB.Driver;
using BCrypt.Net;

namespace CareOS.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly MongoDbContext _context;
        private readonly JwtHelper _jwtHelper;
        private readonly IMongoCollection<User> _users;

        public AuthService(MongoDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
            _users = _context.GetCollection<User>("Users");
        }

        // REGISTER NEW USER
        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                // Check if email already exists (for CARETAKER)
                if (request.Role == "CARETAKER")
                {
                    var existingUser = await _users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
                    if (existingUser != null)
                    {
                        return ApiResponse<AuthResponseDto>.ErrorResponse("Email already registered");
                    }
                }

                // Check if PIN already exists (for ELDER)
                if (request.Role == "ELDER" && !string.IsNullOrEmpty(request.Pin))
                {
                    var existingPin = await _users.Find(u => u.Pin == request.Pin).FirstOrDefaultAsync();
                    if (existingPin != null)
                    {
                        return ApiResponse<AuthResponseDto>.ErrorResponse("PIN already in use");
                    }
                }

                // Create new user
                var user = new User
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Pin = request.Pin,
                    Role = request.Role.ToUpper(),
                    PhoneNumber = request.PhoneNumber,
                    DateOfBirth = request.DateOfBirth,
                    EmergencyContact = request.EmergencyContact,
                    CreatedAt = DateTime.UtcNow
                };

                await _users.InsertOneAsync(user);

                // Generate JWT token
                var token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role);

                var response = new AuthResponseDto
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = user.Role,
                        PhoneNumber = user.PhoneNumber,
                        DateOfBirth = user.DateOfBirth
                    },
                    Message = "Registration successful"
                };

                return ApiResponse<AuthResponseDto>.SuccessResponse(response, "User registered successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponseDto>.ErrorResponse($"Registration failed: {ex.Message}");
            }
        }

        // LOGIN USER
        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
        {
            try
            {
                User? user = null;

                // ELDER LOGIN (PIN based)
                if (request.Role == "ELDER" && !string.IsNullOrEmpty(request.Pin))
                {
                    user = await _users.Find(u => u.Pin == request.Pin && u.Role == "ELDER").FirstOrDefaultAsync();

                    if (user == null)
                    {
                        return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid PIN");
                    }
                }
                // CARETAKER LOGIN (Email + Password)
                else if (request.Role == "CARETAKER")
                {
                    user = await _users.Find(u => u.Email == request.Email && u.Role == "CARETAKER").FirstOrDefaultAsync();

                    if (user == null)
                    {
                        return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password");
                    }

                    // Verify password
                    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                    if (!isPasswordValid)
                    {
                        return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid email or password");
                    }
                }
                else
                {
                    return ApiResponse<AuthResponseDto>.ErrorResponse("Invalid login credentials");
                }

                // Update last login
                var update = Builders<User>.Update.Set(u => u.LastLogin, DateTime.UtcNow);
                await _users.UpdateOneAsync(u => u.Id == user.Id, update);

                // Generate JWT token
                var token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role);

                var response = new AuthResponseDto
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = user.Role,
                        PhoneNumber = user.PhoneNumber,
                        ProfilePicture = user.ProfilePicture,
                        DateOfBirth = user.DateOfBirth
                    },
                    Message = "Login successful"
                };

                return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Login successful");
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponseDto>.ErrorResponse($"Login failed: {ex.Message}");
            }
        }

        // GET USER BY ID
        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("User not found");
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    PhoneNumber = user.PhoneNumber,
                    ProfilePicture = user.ProfilePicture,
                    DateOfBirth = user.DateOfBirth
                };

                return ApiResponse<UserDto>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // UPDATE LAST LOGIN
        public async Task<ApiResponse<bool>> UpdateLastLoginAsync(string userId)
        {
            try
            {
                var update = Builders<User>.Update.Set(u => u.LastLogin, DateTime.UtcNow);
                var result = await _users.UpdateOneAsync(u => u.Id == userId, update);

                return ApiResponse<bool>.SuccessResponse(result.ModifiedCount > 0);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }
        public async Task<ApiResponse<string>> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

                if (user == null)
                {
                    // Security: Don't reveal if email exists
                    return ApiResponse<string>.SuccessResponse("", "If email exists, reset code has been sent");
                }

                // Generate 6-digit code
                var resetCode = new Random().Next(100000, 999999).ToString();

                // Store in temporary collection (create IMongoCollection in constructor)
                var resetTokensCollection = _context.GetCollection<PasswordResetToken>("PasswordResetTokens");

                var resetToken = new PasswordResetToken
                {
                    UserId = user.Id,
                    Email = email,
                    ResetCode = resetCode,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                };

                await resetTokensCollection.InsertOneAsync(resetToken);

                // TODO: Send email/SMS with reset code
                // await _emailService.SendResetCodeAsync(email, resetCode);

                // FOR DEVELOPMENT: Return code (REMOVE in production)
                return ApiResponse<string>.SuccessResponse(
                    resetCode,
                    "Reset code generated (Development mode - remove in production)"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // VERIFY RESET CODE
        public async Task<ApiResponse<bool>> VerifyResetCodeAsync(string email, string code)
        {
            try
            {
                var resetTokensCollection = _context.GetCollection<PasswordResetToken>("PasswordResetTokens");

                var token = await resetTokensCollection.Find(t =>
                    t.Email == email &&
                    t.ResetCode == code &&
                    !t.IsUsed &&
                    t.ExpiresAt > DateTime.UtcNow
                ).FirstOrDefaultAsync();

                if (token == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Invalid or expired reset code");
                }

                return ApiResponse<bool>.SuccessResponse(true, "Code verified");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // RESET PASSWORD
        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto request)
        {
            try
            {
                var resetTokensCollection = _context.GetCollection<PasswordResetToken>("PasswordResetTokens");

                // Verify code
                var token = await resetTokensCollection.Find(t =>
                    t.Email == request.Email &&
                    t.ResetCode == request.ResetCode &&
                    !t.IsUsed &&
                    t.ExpiresAt > DateTime.UtcNow
                ).FirstOrDefaultAsync();

                if (token == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Invalid or expired reset code");
                }

                // Update password
                var user = await _users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                var update = Builders<User>.Update.Set(u => u.PasswordHash, newPasswordHash);
                await _users.UpdateOneAsync(u => u.Id == user.Id, update);

                // Mark token as used
                var tokenUpdate = Builders<PasswordResetToken>.Update.Set(t => t.IsUsed, true);
                await resetTokensCollection.UpdateOneAsync(t => t.Id == token.Id, tokenUpdate);

                return ApiResponse<bool>.SuccessResponse(true, "Password reset successful");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // CHANGE PASSWORD
        public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto request)
        {
            try
            {
                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("User not found");
                }

                // Verify current password
                bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
                if (!isCurrentPasswordValid)
                {
                    return ApiResponse<bool>.ErrorResponse("Current password is incorrect");
                }

                // Update to new password
                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                var update = Builders<User>.Update.Set(u => u.PasswordHash, newPasswordHash);
                await _users.UpdateOneAsync(u => u.Id == userId, update);

                return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // UPDATE PROFILE
        public async Task<ApiResponse<UserDto>> UpdateProfileAsync(string userId, UpdateProfileDto request)
        {
            try
            {
                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("User not found");
                }

                var update = Builders<User>.Update
                    .Set(u => u.FullName, request.FullName)
                    .Set(u => u.PhoneNumber, request.PhoneNumber)
                    .Set(u => u.DateOfBirth, request.DateOfBirth)
                    .Set(u => u.EmergencyContact, request.EmergencyContact);

                if (!string.IsNullOrEmpty(request.ProfilePicture))
                {
                    update = update.Set(u => u.ProfilePicture, request.ProfilePicture);
                }

                await _users.UpdateOneAsync(u => u.Id == userId, update);

                // Get updated user
                var updatedUser = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

                var userDto = new UserDto
                {
                    Id = updatedUser.Id,
                    FullName = updatedUser.FullName,
                    Email = updatedUser.Email,
                    Role = updatedUser.Role,
                    PhoneNumber = updatedUser.PhoneNumber,
                    ProfilePicture = updatedUser.ProfilePicture,
                    DateOfBirth = updatedUser.DateOfBirth
                };

                return ApiResponse<UserDto>.SuccessResponse(userDto, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserDto>.ErrorResponse($"Error: {ex.Message}");
            }
        }
    }

}