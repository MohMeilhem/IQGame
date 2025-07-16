using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IQGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDisableMCQToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DisableMCQ",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisableMCQ",
                table: "Categories");
        }
    }
}
