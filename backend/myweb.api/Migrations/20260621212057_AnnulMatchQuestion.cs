using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myweb.api.Migrations
{
    /// <inheritdoc />
    public partial class AnnulMatchQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAnnulled",
                table: "MatchQuestions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAnnulled",
                table: "MatchQuestions");
        }
    }
}
