using Microsoft.EntityFrameworkCore;
using myweb.api.Data;
using myweb.api.Models;

namespace myweb.api.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthService(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new RegisterResponse
            {
                Email = request.Email ?? string.Empty,
                Success = false,
                Message = "E-post og passord er påkrevd"
            };
        }

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return new RegisterResponse
            {
                Email = request.Email,
                Success = false,
                Message = "E-posten er allerede registrert"
            };
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User
        {
            Email = request.Email,
            PasswordHash = hashedPassword,
            Role = UserRoles.User
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email,
            Success = true,
            Message = "Bruker registrert"
        };
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        return new LoginResponse
        {
            Token = _tokenService.CreateToken(user),
            Email = user.Email,
            Role = user.Role
        };
    }
}
