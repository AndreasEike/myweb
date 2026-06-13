using System.Security.Claims;
using myweb.api.Models;
using myweb.api.Services;

namespace myweb.api.Endpoints;

public static class QuizEndpoints
{
    public static void MapQuizEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization();

        group.MapGet("/matches", (ClaimsPrincipal user, QuizService quiz) =>
            quiz.GetMatchesAsync(UserId(user)));

        group.MapGet("/matches/{id:int}/quiz", async (int id, ClaimsPrincipal user, QuizService quiz) =>
            (await quiz.GetQuizAsync(id, UserId(user))).ToHttp());

        group.MapPut("/matches/{id:int}/answers", async (int id, List<SubmitAnswerEntry> entries, ClaimsPrincipal user, QuizService quiz) =>
            (await quiz.SubmitAnswersAsync(id, UserId(user), entries)).ToHttp());

        group.MapGet("/matches/{id:int}/leaderboard", async (int id, QuizService quiz) =>
            (await quiz.GetMatchLeaderboardAsync(id)).ToHttp());

        group.MapGet("/leaderboard", (QuizService quiz) =>
            quiz.GetOverallLeaderboardAsync());
    }

    private static int UserId(ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue("sub")!);
}
