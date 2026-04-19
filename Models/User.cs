namespace JunkanBackend.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Email, string Role);
public record RegisterRequest(string Email, string Password);