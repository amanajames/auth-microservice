namespace AuthService.Application.DTOs.Request
{
    public record RefreshTokenRequest
    (
        string AccessToken,
        string RefreshToken
    );
}
