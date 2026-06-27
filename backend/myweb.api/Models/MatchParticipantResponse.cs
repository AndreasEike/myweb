namespace myweb.api.Models;

public class MatchParticipantResponse
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public int AnsweredCount { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
}
