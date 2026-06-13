using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using myweb.api.Data;
using myweb.api.Endpoints;
using myweb.api.Models;
using myweb.api.Services;

var builder = WebApplication.CreateBuilder(args);

const string AngularDevClient = "AngularDevClient";

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key is not configured (set the Jwt__Key app setting in production)");

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<QuestionBankService>();
builder.Services.AddScoped<MatchAdminService>();
builder.Services.AddScoped<QuizService>();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevClient, policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            NameClaimType = "sub",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy("Admin", policy => policy.RequireRole(UserRoles.Admin)));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// The SQLite folder may not exist yet on a fresh host (e.g. /home/data on Azure)
var dbPath = new SqliteConnectionStringBuilder(connectionString).DataSource;
var dbDirectory = Path.GetDirectoryName(Path.GetFullPath(dbPath));
if (!string.IsNullOrEmpty(dbDirectory))
{
    Directory.CreateDirectory(dbDirectory);
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    foreach (var legacyUser in db.Users.Where(u => u.Role == ""))
    {
        legacyUser.Role = UserRoles.User;
    }

    var adminEmail = app.Configuration["Seed:AdminEmail"];
    var adminPassword = app.Configuration["Seed:AdminPassword"];
    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var admin = db.Users.FirstOrDefault(u => u.Email == adminEmail);
        if (admin == null)
        {
            db.Users.Add(new User
            {
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Role = UserRoles.Admin
            });
        }
        else
        {
            admin.Role = UserRoles.Admin;
        }
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors(AngularDevClient);
}

app.UseHttpsRedirection();

// Serve the Angular build from wwwroot (populated by `dotnet publish`)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/health", () => new { status = "ok" });

app.MapAuthEndpoints();
app.MapAdminEndpoints();
app.MapQuizEndpoints();

// Unmatched non-API routes go to the SPA so Angular routing works on refresh
app.MapFallbackToFile("index.html");

app.Run();
