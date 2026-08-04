namespace AuthService.Application.DTOs.Request
{
    public record UserDto
    (
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string Role,
        DateTime CreatedAt
    );
}
