namespace myweb.api.Models;

public class LeaderboardEntryResponse
{
    public int Rank { get; set; }
    public required string Name { get; set; }
    public int Points { get; set; }
    public int MatchesPlayed { get; set; }
}
