using Microsoft.EntityFrameworkCore;
using myweb.api.Models;

namespace myweb.api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<MatchQuestion> MatchQuestions { get; set; }
    public DbSet<UserAnswer> UserAnswers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SQLite has no datetime type; values come back with Kind=Unspecified,
        // which serializes without the Z suffix and is misread as local time
        // by clients. Mark everything stored here as UTC on the way out.
        modelBuilder.Entity<Match>()
            .Property(m => m.KickoffUtc)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        modelBuilder.Entity<UserAnswer>()
            .Property(ua => ua.UpdatedAtUtc)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        modelBuilder.Entity<MatchQuestion>(entity =>
        {
            entity.HasIndex(mq => new { mq.MatchId, mq.OrderIndex }).IsUnique();
            entity.HasOne(mq => mq.Match)
                  .WithMany(m => m.Questions)
                  .HasForeignKey(mq => mq.MatchId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(mq => mq.Question)
                  .WithMany()
                  .HasForeignKey(mq => mq.QuestionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasIndex(ua => new { ua.UserId, ua.MatchQuestionId }).IsUnique();
            entity.HasOne(ua => ua.User)
                  .WithMany()
                  .HasForeignKey(ua => ua.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ua => ua.MatchQuestion)
                  .WithMany(mq => mq.Answers)
                  .HasForeignKey(ua => ua.MatchQuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
