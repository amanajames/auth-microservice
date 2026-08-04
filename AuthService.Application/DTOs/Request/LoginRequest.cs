namespace AuthService.Application.DTOs.Request
{
    public record LoginRequest
    (
        string Email,
        string Password
    );
}
