using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentApprovalSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentsToVotesAndConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "ApprovalVotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CommentsRequired",
                table: "ApprovalConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comments",
                table: "ApprovalVotes");

            migrationBuilder.DropColumn(
                name: "CommentsRequired",
                table: "ApprovalConfigs");
        }
    }
}
