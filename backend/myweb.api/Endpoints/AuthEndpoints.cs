using System.Security.Claims;
using myweb.api.Models;
using myweb.api.Services;

namespace myweb.api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (RegisterRequest request, AuthService authService) =>
        {
            var response = await authService.RegisterAsync(request);
            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        });

        group.MapPost("/login", async (LoginRequest request, AuthService authService) =>
        {
            var response = await authService.LoginAsync(request);
            return response == null
                ? Results.Json(new { message = "Feil e-post eller passord" }, statusCode: StatusCodes.Status401Unauthorized)
                : Results.Ok(response);
        });

        group.MapGet("/me", (ClaimsPrincipal user) => new CurrentUserResponse
        {
            Id = int.Parse(user.FindFirstValue("sub")!),
            Email = user.FindFirstValue("email") ?? string.Empty,
            Role = user.FindFirstValue("role") ?? UserRoles.User
        }).RequireAuthorization();
    }
}
