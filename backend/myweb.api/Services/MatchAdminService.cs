using Microsoft.EntityFrameworkCore;
using myweb.api.Data;
using myweb.api.Models;

namespace myweb.api.Services;

public class MatchAdminService(AppDbContext db)
{
    public const int QuestionsPerMatch = 20;

    public async Task<List<AdminMatchResponse>> GetMatchesAsync()
    {
        var matches = await db.Matches
            .Include(m => m.Questions)
            .OrderBy(m => m.KickoffUtc)
            .ToListAsync();
        return matches.Select(ToResponse).ToList();
    }

    public async Task<Result<AdminMatchResponse>> CreateMatchAsync(MatchRequest request)
    {
        var error = Validate(request);
        if (error != null)
        {
            return Result<AdminMatchResponse>.Fail(error);
        }

        var match = new Match
        {
            HomeTeam = request.HomeTeam.Trim(),
            AwayTeam = request.AwayTeam.Trim(),
            KickoffUtc = AsUtc(request.KickoffUtc)
        };
        db.Matches.Add(match);
        await db.SaveChangesAsync();

        return Result<AdminMatchResponse>.Success(ToResponse(match));
    }

    public async Task<Result<AdminMatchResponse>> UpdateMatchAsync(int id, MatchRequest request)
    {
        var match = await db.Matches.Include(m => m.Questions).FirstOrDefaultAsync(m => m.Id == id);
        if (match == null)
        {
            return Result<AdminMatchResponse>.Fail("Kampen finnes ikke", 404);
        }

        var error = Validate(request);
        if (error != null)
        {
            return Result<AdminMatchResponse>.Fail(error);
        }

        match.HomeTeam = request.HomeTeam.Trim();
        match.AwayTeam = request.AwayTeam.Trim();
        match.KickoffUtc = AsUtc(request.KickoffUtc);
        await db.SaveChangesAsync();

        return Result<AdminMatchResponse>.Success(ToResponse(match));
    }

    public async Task<Result<bool>> DeleteMatchAsync(int id)
    {
        var match = await db.Matches.FindAsync(id);
        if (match == null)
        {
            return Result<bool>.Fail("Kampen finnes ikke", 404);
        }

        db.Matches.Remove(match);
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<List<MatchQuestionResponse>>> GetMatchQuestionsAsync(int matchId)
    {
        var exists = await db.Matches.AnyAsync(m => m.Id == matchId);
        if (!exists)
        {
            return Result<List<MatchQuestionResponse>>.Fail("Kampen finnes ikke", 404);
        }

        return Result<List<MatchQuestionResponse>>.Success(await LoadQuestionsAsync(matchId));
    }

    /// <summary>
    /// Replaces the match's question list. Existing assignments are matched on
    /// (QuestionId, WildcardValue) so user answers survive reordering and partial edits.
    /// </summary>
    public async Task<Result<List<MatchQuestionResponse>>> SetMatchQuestionsAsync(
        int matchId, List<AssignmentEntry> entries)
    {
        var match = await db.Matches.Include(m => m.Questions)
            .FirstOrDefaultAsync(m => m.Id == matchId);
        if (match == null)
        {
            return Result<List<MatchQuestionResponse>>.Fail("Kampen finnes ikke", 404);
        }
        if (match.Questions.Any(q => q.CorrectAnswer != null || q.IsAnnulled))
        {
            return Result<List<MatchQuestionResponse>>.Fail(
                "Fasit er registrert for kampen – spørsmålene kan ikke endres", 409);
        }
        if (entries.Count > QuestionsPerMatch)
        {
            return Result<List<MatchQuestionResponse>>.Fail(
                $"En kamp kan ha maks {QuestionsPerMatch} spørsmål");
        }
        if (entries.Select(e => e.OrderIndex).Distinct().Count() != entries.Count
            || entries.Any(e => e.OrderIndex < 1 || e.OrderIndex > QuestionsPerMatch))
        {
            return Result<List<MatchQuestionResponse>>.Fail("Ugyldig rekkefølge på spørsmålene");
        }

        var questionIds = entries.Select(e => e.QuestionId).Distinct().ToList();
        var bank = await db.Questions
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);

        foreach (var entry in entries)
        {
            if (!bank.TryGetValue(entry.QuestionId, out var question))
            {
                return Result<List<MatchQuestionResponse>>.Fail(
                    $"Spørsmål {entry.QuestionId} finnes ikke i banken");
            }

            var wildcard = string.IsNullOrWhiteSpace(entry.WildcardValue)
                ? null
                : entry.WildcardValue.Trim();
            if (question.HasWildcard && wildcard == null)
            {
                return Result<List<MatchQuestionResponse>>.Fail(
                    $"Spørsmålet «{question.Text}» krever et spillernavn");
            }
            entry.WildcardValue = question.HasWildcard ? wildcard : null;
        }

        if (entries.GroupBy(e => (e.QuestionId, e.WildcardValue)).Any(g => g.Count() > 1))
        {
            return Result<List<MatchQuestionResponse>>.Fail(
                "Samme spørsmål kan ikke brukes flere ganger med samme spiller");
        }

        var remaining = match.Questions.ToList();
        var kept = new List<(MatchQuestion Assigned, AssignmentEntry Entry)>();
        var added = new List<AssignmentEntry>();
        foreach (var entry in entries)
        {
            var existing = remaining.FirstOrDefault(mq =>
                mq.QuestionId == entry.QuestionId && mq.WildcardValue == entry.WildcardValue);
            if (existing != null)
            {
                remaining.Remove(existing);
                kept.Add((existing, entry));
            }
            else
            {
                added.Add(entry);
            }
        }

        await using var transaction = await db.Database.BeginTransactionAsync();

        // Park kept rows on negative order indexes first so the unique
        // (MatchId, OrderIndex) index never collides while reordering.
        db.MatchQuestions.RemoveRange(remaining);
        var parkingIndex = -1;
        foreach (var (assigned, _) in kept)
        {
            assigned.OrderIndex = parkingIndex--;
        }
        await db.SaveChangesAsync();

        foreach (var (assigned, entry) in kept)
        {
            assigned.OrderIndex = entry.OrderIndex;
        }
        foreach (var entry in added)
        {
            db.MatchQuestions.Add(new MatchQuestion
            {
                MatchId = matchId,
                QuestionId = entry.QuestionId,
                OrderIndex = entry.OrderIndex,
                WildcardValue = entry.WildcardValue
            });
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Result<List<MatchQuestionResponse>>.Success(await LoadQuestionsAsync(matchId));
    }

    /// <summary>
    /// Stores the answer key for every question in the match, then scores all
    /// user answers in one pass. Re-running with a corrected key re-scores.
    /// </summary>
    public async Task<Result<AnswerKeyResponse>> SetAnswerKeyAsync(
        int matchId, List<AnswerKeyEntry> entries)
    {
        var matchQuestions = await db.MatchQuestions
            .Include(mq => mq.Question)
            .Where(mq => mq.MatchId == matchId)
            .ToListAsync();
        if (matchQuestions.Count == 0)
        {
            var matchExists = await db.Matches.AnyAsync(m => m.Id == matchId);
            return Result<AnswerKeyResponse>.Fail(
                matchExists ? "Kampen har ingen spørsmål" : "Kampen finnes ikke",
                matchExists ? 400 : 404);
        }

        var byId = matchQuestions.ToDictionary(mq => mq.Id);
        var seen = new HashSet<int>();
        foreach (var entry in entries)
        {
            if (!byId.TryGetValue(entry.MatchQuestionId, out var matchQuestion))
            {
                return Result<AnswerKeyResponse>.Fail(
                    $"Spørsmål {entry.MatchQuestionId} hører ikke til kampen");
            }
            if (!seen.Add(entry.MatchQuestionId))
            {
                return Result<AnswerKeyResponse>.Fail("Samme spørsmål er angitt flere ganger");
            }

            if (entry.IsAnnulled)
            {
                matchQuestion.IsAnnulled = true;
                matchQuestion.CorrectAnswer = null;
                continue;
            }

            var normalized = AnswerFormats.Normalize(matchQuestion.Question.Type, entry.CorrectAnswer);
            if (normalized == null)
            {
                return Result<AnswerKeyResponse>.Fail(
                    $"Ugyldig svar for spørsmål {matchQuestion.OrderIndex}");
            }
            matchQuestion.IsAnnulled = false;
            matchQuestion.CorrectAnswer = normalized;
        }

        var allAnswered = matchQuestions.All(mq => mq.CorrectAnswer != null || mq.IsAnnulled);
        int scoredAnswers = 0;
        int participants = 0;

        if (allAnswered)
        {
            var answers = await db.UserAnswers
                .Include(ua => ua.MatchQuestion)
                .Where(ua => ua.MatchQuestion.MatchId == matchId)
                .ToListAsync();
            foreach (var answer in answers)
            {
                answer.IsCorrect = answer.Answer == answer.MatchQuestion.CorrectAnswer;
            }
            scoredAnswers = answers.Count;
            participants = answers.Select(a => a.UserId).Distinct().Count();
        }

        await db.SaveChangesAsync();

        return Result<AnswerKeyResponse>.Success(new AnswerKeyResponse
        {
            QuestionsKeyed = seen.Count,
            ScoredAnswers = scoredAnswers,
            Participants = participants,
            Published = allAnswered
        });
    }

    private static string? Validate(MatchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.HomeTeam) || string.IsNullOrWhiteSpace(request.AwayTeam))
        {
            return "Begge lag er påkrevd";
        }
        if (request.KickoffUtc == default)
        {
            return "Avsparktidspunkt er påkrevd";
        }
        return null;
    }

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static AdminMatchResponse ToResponse(Match match) => new()
    {
        Id = match.Id,
        HomeTeam = match.HomeTeam,
        AwayTeam = match.AwayTeam,
        KickoffUtc = match.KickoffUtc,
        QuestionCount = match.Questions.Count,
        HasAnswerKey = match.Questions.Count > 0
            && match.Questions.All(q => q.CorrectAnswer != null || q.IsAnnulled),
        IsLocked = match.IsLocked
    };

    private async Task<List<MatchQuestionResponse>> LoadQuestionsAsync(int matchId)
    {
        var assigned = await db.MatchQuestions
            .Include(mq => mq.Question)
            .Where(mq => mq.MatchId == matchId)
            .OrderBy(mq => mq.OrderIndex)
            .ToListAsync();

        return assigned.Select(mq => new MatchQuestionResponse
        {
            MatchQuestionId = mq.Id,
            QuestionId = mq.QuestionId,
            OrderIndex = mq.OrderIndex,
            Text = mq.Question.Text,
            ResolvedText = mq.WildcardValue == null
                ? mq.Question.Text
                : mq.Question.Text.Replace("{X}", mq.WildcardValue),
            Type = mq.Question.Type,
            HasWildcard = mq.Question.HasWildcard,
            WildcardValue = mq.WildcardValue,
            CorrectAnswer = mq.CorrectAnswer,
            IsAnnulled = mq.IsAnnulled
        }).ToList();
    }
}
