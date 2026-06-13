namespace myweb.api.Models;

public class Match
{
    public const int LockMinutesBeforeKickoff = 5;

    public int Id { get; set; }
    public required string HomeTeam { get; set; }
    public required string AwayTeam { get; set; }
    public DateTime KickoffUtc { get; set; }
    public List<MatchQuestion> Questions { get; set; } = [];

    public bool IsLocked => DateTime.UtcNow >= KickoffUtc.AddMinutes(-LockMinutesBeforeKickoff);
}
