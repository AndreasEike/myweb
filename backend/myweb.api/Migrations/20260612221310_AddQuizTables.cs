using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myweb.api.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HomeTeam = table.Column<string>(type: "TEXT", nullable: false),
                    AwayTeam = table.Column<string>(type: "TEXT", nullable: false),
                    KickoffUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    HasWildcard = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MatchId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    WildcardValue = table.Column<string>(type: "TEXT", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchQuestions_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchQuestions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchQuestionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Answer = table.Column<string>(type: "TEXT", nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAnswers_MatchQuestions_MatchQuestionId",
                        column: x => x.MatchQuestionId,
                        principalTable: "MatchQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAnswers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchQuestions_MatchId_OrderIndex",
                table: "MatchQuestions",
                columns: new[] { "MatchId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchQuestions_QuestionId",
                table: "MatchQuestions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_MatchQuestionId",
                table: "UserAnswers",
                column: "MatchQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_UserId_MatchQuestionId",
                table: "UserAnswers",
                columns: new[] { "UserId", "MatchQuestionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAnswers");

            migrationBuilder.DropTable(
                name: "MatchQuestions");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Questions");
        }
    }
}
