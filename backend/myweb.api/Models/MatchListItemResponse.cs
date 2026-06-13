namespace myweb.api.Models;

public class MatchListItemResponse
{
    public int Id { get; set; }
    public required string HomeTeam { get; set; }
    public required string AwayTeam { get; set; }
    public DateTime KickoffUtc { get; set; }
    public DateTime LockAtUtc { get; set; }
    public required string Status { get; set; }
    public int AnsweredCount { get; set; }
    public int QuestionCount { get; set; }
    public int? MyPoints { get; set; }
}
