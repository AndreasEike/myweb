using myweb.api.Models;
using myweb.api.Services;

namespace myweb.api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin").RequireAuthorization("Admin");

        group.MapGet("/questions", (QuestionBankService bank) =>
            bank.GetAllAsync());

        group.MapPost("/questions", async (QuestionRequest request, QuestionBankService bank) =>
            (await bank.CreateAsync(request)).ToHttp());

        group.MapPut("/questions/{id:int}", async (int id, QuestionRequest request, QuestionBankService bank) =>
            (await bank.UpdateAsync(id, request)).ToHttp());

        group.MapDelete("/questions/{id:int}", async (int id, QuestionBankService bank) =>
            (await bank.DeleteAsync(id)).ToHttp());

        group.MapGet("/matches", (MatchAdminService matches) =>
            matches.GetMatchesAsync());

        group.MapPost("/matches", async (MatchRequest request, MatchAdminService matches) =>
            (await matches.CreateMatchAsync(request)).ToHttp());

        group.MapPut("/matches/{id:int}", async (int id, MatchRequest request, MatchAdminService matches) =>
            (await matches.UpdateMatchAsync(id, request)).ToHttp());

        group.MapDelete("/matches/{id:int}", async (int id, MatchAdminService matches) =>
            (await matches.DeleteMatchAsync(id)).ToHttp());

        group.MapGet("/matches/{id:int}/questions", async (int id, MatchAdminService matches) =>
            (await matches.GetMatchQuestionsAsync(id)).ToHttp());

        group.MapGet("/matches/{id:int}/participants", async (int id, MatchAdminService matches) =>
            (await matches.GetParticipantsAsync(id)).ToHttp());

        group.MapPut("/matches/{id:int}/questions", async (int id, List<AssignmentEntry> entries, MatchAdminService matches) =>
            (await matches.SetMatchQuestionsAsync(id, entries)).ToHttp());

        group.MapPut("/matches/{id:int}/answer-key", async (int id, List<AnswerKeyEntry> entries, MatchAdminService matches) =>
            (await matches.SetAnswerKeyAsync(id, entries)).ToHttp());
    }
}
