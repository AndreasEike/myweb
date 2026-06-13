namespace myweb.api.Models;

public class AdminMatchResponse
{
    public int Id { get; set; }
    public required string HomeTeam { get; set; }
    public required string AwayTeam { get; set; }
    public DateTime KickoffUtc { get; set; }
    public int QuestionCount { get; set; }
    public bool HasAnswerKey { get; set; }
    public bool IsLocked { get; set; }
}
