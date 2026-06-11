using myweb.api.Data;
using myweb.api.Models;

namespace myweb.api.Services;

public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new RegisterResponse
            {
                Email = request.Email ?? string.Empty,
                Success = false,
                Message = "Email and password are required"
            };
        }

        var existingUser = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return new RegisterResponse
            {
                Email = request.Email,
                Success = false,
                Message = "Email already registered"
            };
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User
        {
            Email = request.Email,
            PasswordHash = hashedPassword
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email,
            Success = true,
            Message = "User registered successfully"
        };
    }
}
