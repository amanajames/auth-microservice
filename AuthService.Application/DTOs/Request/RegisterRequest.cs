namespace AuthService.Application.DTOs.Request
{
    public record RegisterRequest
    (
        string FirstName,
        string LastName,
        string Email,
        string Password

    );
}