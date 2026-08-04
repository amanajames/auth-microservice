namespace AuthService.Application.DTOs.Request
{
    public record ChangePasswordRequest
    (
        string CurrentPassword,
        string NewPassword
    );
}
