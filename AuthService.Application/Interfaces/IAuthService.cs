
using AuthService.Application.DTOs.Request;
using AuthService.Application.DTOs.Response;

namespace AuthService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
        Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
        Task<ApiResponse<bool>> RevokeTokenAsync(Guid userId);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId);
    }
}
