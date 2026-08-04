
using AuthService.Application.DTOs.Request;
using AuthService.Application.DTOs.Response;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using System.Security.Claims;

namespace AuthService.Application.Services
{
    public class AuthServiceImpl : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public AuthServiceImpl(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
        {
            if (await _userRepository.ExistsAsync(request.Email))
                return new ApiResponse<AuthResponse>(false, "Email already exists.");

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RefreshToken = _tokenService.GenerateRefreshToken(),
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
            };

            await _userRepository.CreateAsync(user);

            var accessToken = _tokenService.GenerateAccessToken(user);
            return new ApiResponse<AuthResponse>(true, "Registration successful.", BuildAuthResponse(user, accessToken));
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email.ToLower());

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return new ApiResponse<AuthResponse>(false, "Invalid email or password.");

            if (!user.IsActive)
                return new ApiResponse<AuthResponse>(false, "Account is deactivated.");

            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var accessToken = _tokenService.GenerateAccessToken(user);
            return new ApiResponse<AuthResponse>(true, "Login successful.", BuildAuthResponse(user, accessToken));
        }

        public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal is null)
                return new ApiResponse<AuthResponse>(false, "Invalid access token.");

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null)
                return new ApiResponse<AuthResponse>(false, "Invalid token claims.");

            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);
            if (user is null || user.Id.ToString() != userId || user.RefreshTokenExpiry < DateTime.UtcNow)
                return new ApiResponse<AuthResponse>(false, "Invalid or expired refresh token.");

            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var accessToken = _tokenService.GenerateAccessToken(user);
            return new ApiResponse<AuthResponse>(true, "Token refreshed.", BuildAuthResponse(user, accessToken));
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                return new ApiResponse<bool>(false, "User not found.");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return new ApiResponse<bool>(false, "Current password is incorrect.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new ApiResponse<bool>(true, "Password changed successfully.", true);
        }

        public async Task<ApiResponse<bool>> RevokeTokenAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                return new ApiResponse<bool>(false, "User not found.");

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new ApiResponse<bool>(true, "Token revoked successfully.", true);
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                return new ApiResponse<UserDto>(false, "User not found.");

            return new ApiResponse<UserDto>(true, "User found.", MapToDto(user));
        }

        private static AuthResponse BuildAuthResponse(User user, string accessToken) =>
            new(accessToken, user.RefreshToken!, DateTime.UtcNow.AddMinutes(60), MapToDto(user));

        private static UserDto MapToDto(User user) =>
            new(user.Id, user.FirstName, user.LastName, user.Email, user.Role, user.CreatedAt);
    }
}
