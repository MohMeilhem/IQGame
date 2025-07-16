using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IQGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "Categories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Group",
                table: "Categories");
        }
    }
}
