namespace myweb.api.Models;

public class QuizResponse
{
    public int MatchId { get; set; }
    public required string HomeTeam { get; set; }
    public required string AwayTeam { get; set; }
    public DateTime KickoffUtc { get; set; }
    public DateTime LockAtUtc { get; set; }
    public bool IsLocked { get; set; }
    public bool HasAnswerKey { get; set; }
    public int? MyPoints { get; set; }
    public List<QuizQuestionResponse> Questions { get; set; } = [];
}
