using JunkanBackend.Models;

namespace JunkanBackend.Services{
public class UserStore
{
    private readonly List<User> _users = [];
    private int _nextId = 1;

    public User? FindByEmail(string email) =>
        _users.FirstOrDefault(u => u.Email == email);

    public User Create(string email, string password)
    {
        var user = new User
        {
            Id = _nextId++,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "user"
        };
        _users.Add(user);
        return user;
    }

    public bool VerifyPassword(User user, string password) =>
        BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
}
}