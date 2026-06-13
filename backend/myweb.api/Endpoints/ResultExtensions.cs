using myweb.api.Models;

namespace myweb.api.Endpoints;

public static class ResultExtensions
{
    public static IResult ToHttp<T>(this Result<T> result) =>
        result.Ok
            ? Results.Ok(result.Value)
            : Results.Json(new { message = result.Error }, statusCode: result.Status);
}
