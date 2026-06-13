using Microsoft.EntityFrameworkCore;
using myweb.api.Data;
using myweb.api.Models;

namespace myweb.api.Services;

public class QuestionBankService(AppDbContext db)
{
    public async Task<List<QuestionResponse>> GetAllAsync() =>
        await db.Questions
            .OrderBy(q => q.Id)
            .Select(q => new QuestionResponse
            {
                Id = q.Id,
                Text = q.Text,
                Type = q.Type,
                HasWildcard = q.HasWildcard,
                UsageCount = db.MatchQuestions.Count(mq => mq.QuestionId == q.Id)
            })
            .ToListAsync();

    public async Task<Result<QuestionResponse>> CreateAsync(QuestionRequest request)
    {
        var error = Validate(request);
        if (error != null)
        {
            return Result<QuestionResponse>.Fail(error);
        }

        var question = new Question
        {
            Text = request.Text.Trim(),
            Type = request.Type,
            HasWildcard = request.HasWildcard
        };
        db.Questions.Add(question);
        await db.SaveChangesAsync();

        return Result<QuestionResponse>.Success(ToResponse(question, 0));
    }

    public async Task<Result<QuestionResponse>> UpdateAsync(int id, QuestionRequest request)
    {
        var question = await db.Questions.FindAsync(id);
        if (question == null)
        {
            return Result<QuestionResponse>.Fail("Spørsmålet finnes ikke", 404);
        }

        var error = Validate(request);
        if (error != null)
        {
            return Result<QuestionResponse>.Fail(error);
        }

        var usageCount = await db.MatchQuestions.CountAsync(mq => mq.QuestionId == id);
        if (usageCount > 0 && (question.Type != request.Type || question.HasWildcard != request.HasWildcard))
        {
            return Result<QuestionResponse>.Fail(
                "Spørsmålet er i bruk i en kamp – type og joker kan ikke endres", 409);
        }

        question.Text = request.Text.Trim();
        question.Type = request.Type;
        question.HasWildcard = request.HasWildcard;
        await db.SaveChangesAsync();

        return Result<QuestionResponse>.Success(ToResponse(question, usageCount));
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var question = await db.Questions.FindAsync(id);
        if (question == null)
        {
            return Result<bool>.Fail("Spørsmålet finnes ikke", 404);
        }

        var inUse = await db.MatchQuestions.AnyAsync(mq => mq.QuestionId == id);
        if (inUse)
        {
            return Result<bool>.Fail("Spørsmålet er i bruk i en kamp og kan ikke slettes", 409);
        }

        db.Questions.Remove(question);
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static string? Validate(QuestionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return "Spørsmålstekst er påkrevd";
        }
        if (!Enum.IsDefined(request.Type))
        {
            return "Ugyldig spørsmålstype";
        }
        if (request.HasWildcard && !request.Text.Contains("{X}"))
        {
            return "Spørsmål med joker må inneholde {X} i teksten";
        }
        if (!request.HasWildcard && request.Text.Contains("{X}"))
        {
            return "Teksten inneholder {X} – merk spørsmålet som jokerspørsmål";
        }
        return null;
    }

    private static QuestionResponse ToResponse(Question question, int usageCount) => new()
    {
        Id = question.Id,
        Text = question.Text,
        Type = question.Type,
        HasWildcard = question.HasWildcard,
        UsageCount = usageCount
    };
}
