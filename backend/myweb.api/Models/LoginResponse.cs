namespace myweb.api.Models;

public class LoginResponse
{
    public required string Token { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
}
