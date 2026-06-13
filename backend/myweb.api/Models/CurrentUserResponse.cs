namespace myweb.api.Models;

public class CurrentUserResponse
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
}
