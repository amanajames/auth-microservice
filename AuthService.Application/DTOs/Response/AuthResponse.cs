using AuthService.Application.DTOs.Request;

namespace AuthService.Application.DTOs.Response
{
    public record AuthResponse
    (
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresAt,
        UserDto User
    );
}
