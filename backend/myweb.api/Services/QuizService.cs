using Microsoft.EntityFrameworkCore;
using myweb.api.Data;
using myweb.api.Models;

namespace myweb.api.Services;

public class QuizService(AppDbContext db)
{
    public const string StatusUpcoming = "upcoming";
    public const string StatusLocked = "locked";
    public const string StatusFinished = "finished";

    public async Task<List<MatchListItemResponse>> GetMatchesAsync(int userId)
    {
        var matches = await db.Matches
            .Include(m => m.Questions)
            .OrderBy(m => m.KickoffUtc)
            .ToListAsync();
        var visible = matches
            .Where(m => m.Questions.Count == MatchAdminService.QuestionsPerMatch)
            .ToList();

        var matchIds = visible.Select(m => m.Id).ToList();
        var myAnswers = await db.UserAnswers
            .Where(ua => ua.UserId == userId && matchIds.Contains(ua.MatchQuestion.MatchId))
            .Select(ua => new { ua.MatchQuestion.MatchId, ua.IsCorrect })
            .ToListAsync();

        return visible.Select(m =>
        {
            var answered = myAnswers.Where(a => a.MatchId == m.Id).ToList();
            var hasKey = m.Questions.All(q => q.CorrectAnswer != null);
            return new MatchListItemResponse
            {
                Id = m.Id,
                HomeTeam = m.HomeTeam,
                AwayTeam = m.AwayTeam,
                KickoffUtc = m.KickoffUtc,
                LockAtUtc = m.KickoffUtc.AddMinutes(-Match.LockMinutesBeforeKickoff),
                Status = hasKey ? StatusFinished : m.IsLocked ? StatusLocked : StatusUpcoming,
                AnsweredCount = answered.Count,
                QuestionCount = m.Questions.Count,
                MyPoints = hasKey ? answered.Count(a => a.IsCorrect == true) : null
            };
        }).ToList();
    }

    public async Task<Result<QuizResponse>> GetQuizAsync(int matchId, int userId)
    {
        var match = await db.Matches
            .Include(m => m.Questions)
            .ThenInclude(mq => mq.Question)
            .FirstOrDefaultAsync(m => m.Id == matchId);
        if (match == null || match.Questions.Count != MatchAdminService.QuestionsPerMatch)
        {
            return Result<QuizResponse>.Fail("Quizen finnes ikke", 404);
        }

        var matchQuestionIds = match.Questions.Select(mq => mq.Id).ToList();
        var myAnswers = await db.UserAnswers
            .Where(ua => ua.UserId == userId && matchQuestionIds.Contains(ua.MatchQuestionId))
            .ToDictionaryAsync(ua => ua.MatchQuestionId);

        var hasKey = match.Questions.All(q => q.CorrectAnswer != null);
        var questions = match.Questions
            .OrderBy(mq => mq.OrderIndex)
            .Select(mq =>
            {
                myAnswers.TryGetValue(mq.Id, out var mine);
                return new QuizQuestionResponse
                {
                    MatchQuestionId = mq.Id,
                    OrderIndex = mq.OrderIndex,
                    Text = mq.WildcardValue == null
                        ? mq.Question.Text
                        : mq.Question.Text.Replace("{X}", mq.WildcardValue),
                    Type = mq.Question.Type,
                    MyAnswer = mine?.Answer,
                    CorrectAnswer = hasKey ? mq.CorrectAnswer : null,
                    IsCorrect = hasKey ? mine?.IsCorrect : null
                };
            })
            .ToList();

        return Result<QuizResponse>.Success(new QuizResponse
        {
            MatchId = match.Id,
            HomeTeam = match.HomeTeam,
            AwayTeam = match.AwayTeam,
            KickoffUtc = match.KickoffUtc,
            LockAtUtc = match.KickoffUtc.AddMinutes(-Match.LockMinutesBeforeKickoff),
            IsLocked = match.IsLocked,
            HasAnswerKey = hasKey,
            MyPoints = hasKey ? questions.Count(q => q.IsCorrect == true) : null,
            Questions = questions
        });
    }

    public async Task<Result<SubmitAnswersResponse>> SubmitAnswersAsync(
        int matchId, int userId, List<SubmitAnswerEntry> entries)
    {
        var match = await db.Matches
            .Include(m => m.Questions)
            .ThenInclude(mq => mq.Question)
            .FirstOrDefaultAsync(m => m.Id == matchId);
        if (match == null || match.Questions.Count != MatchAdminService.QuestionsPerMatch)
        {
            return Result<SubmitAnswersResponse>.Fail("Quizen finnes ikke", 404);
        }
        if (match.IsLocked)
        {
            return Result<SubmitAnswersResponse>.Fail("Fristen er ute – kampen er låst");
        }

        var byId = match.Questions.ToDictionary(mq => mq.Id);
        var normalized = new Dictionary<int, string>();
        foreach (var entry in entries)
        {
            if (!byId.TryGetValue(entry.MatchQuestionId, out var matchQuestion))
            {
                return Result<SubmitAnswersResponse>.Fail(
                    $"Spørsmål {entry.MatchQuestionId} hører ikke til kampen");
            }
            var answer = AnswerFormats.Normalize(matchQuestion.Question.Type, entry.Answer);
            if (answer == null)
            {
                return Result<SubmitAnswersResponse>.Fail(
                    $"Ugyldig svar på spørsmål {matchQuestion.OrderIndex}");
            }
            normalized[entry.MatchQuestionId] = answer;
        }

        var existing = await db.UserAnswers
            .Where(ua => ua.UserId == userId && normalized.Keys.Contains(ua.MatchQuestionId))
            .ToDictionaryAsync(ua => ua.MatchQuestionId);
        var now = DateTime.UtcNow;
        foreach (var (matchQuestionId, answer) in normalized)
        {
            if (existing.TryGetValue(matchQuestionId, out var userAnswer))
            {
                userAnswer.Answer = answer;
                userAnswer.UpdatedAtUtc = now;
            }
            else
            {
                db.UserAnswers.Add(new UserAnswer
                {
                    UserId = userId,
                    MatchQuestionId = matchQuestionId,
                    Answer = answer,
                    UpdatedAtUtc = now
                });
            }
        }
        await db.SaveChangesAsync();

        return Result<SubmitAnswersResponse>.Success(new SubmitAnswersResponse
        {
            Saved = normalized.Count
        });
    }

    public async Task<Result<List<LeaderboardEntryResponse>>> GetMatchLeaderboardAsync(int matchId)
    {
        var match = await db.Matches.Include(m => m.Questions).FirstOrDefaultAsync(m => m.Id == matchId);
        if (match == null || match.Questions.Count == 0)
        {
            return Result<List<LeaderboardEntryResponse>>.Fail("Kampen finnes ikke", 404);
        }
        if (!match.Questions.All(q => q.CorrectAnswer != null))
        {
            return Result<List<LeaderboardEntryResponse>>.Fail(
                "Resultatene er ikke klare ennå", 409);
        }

        var rows = await db.UserAnswers
            .Where(ua => ua.MatchQuestion.MatchId == matchId)
            .GroupBy(ua => new { ua.UserId, ua.User.Email })
            .Select(g => new
            {
                g.Key.Email,
                Points = g.Count(ua => ua.IsCorrect == true)
            })
            .ToListAsync();

        return Result<List<LeaderboardEntryResponse>>.Success(Rank(rows.Select(r =>
            (r.Email, r.Points, MatchesPlayed: 1))));
    }

    public async Task<List<LeaderboardEntryResponse>> GetOverallLeaderboardAsync()
    {
        var rows = await db.UserAnswers
            .Where(ua => ua.IsCorrect != null)
            .GroupBy(ua => new { ua.UserId, ua.User.Email })
            .Select(g => new
            {
                g.Key.Email,
                Points = g.Count(ua => ua.IsCorrect == true),
                MatchesPlayed = g.Select(ua => ua.MatchQuestion.MatchId).Distinct().Count()
            })
            .ToListAsync();

        return Rank(rows.Select(r => (r.Email, r.Points, r.MatchesPlayed)));
    }

    private static List<LeaderboardEntryResponse> Rank(
        IEnumerable<(string Email, int Points, int MatchesPlayed)> rows)
    {
        var ordered = rows
            .OrderByDescending(r => r.Points)
            .ThenBy(r => r.Email)
            .ToList();

        var result = new List<LeaderboardEntryResponse>();
        var rank = 0;
        var previousPoints = int.MinValue;
        for (var i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].Points != previousPoints)
            {
                rank = i + 1;
                previousPoints = ordered[i].Points;
            }
            result.Add(new LeaderboardEntryResponse
            {
                Rank = rank,
                Name = ordered[i].Email.Split('@')[0],
                Points = ordered[i].Points,
                MatchesPlayed = ordered[i].MatchesPlayed
            });
        }
        return result;
    }
}
