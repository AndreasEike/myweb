namespace myweb.api.Models;

public class MatchRequest
{
    public required string HomeTeam { get; set; }
    public required string AwayTeam { get; set; }
    public DateTime KickoffUtc { get; set; }
}
