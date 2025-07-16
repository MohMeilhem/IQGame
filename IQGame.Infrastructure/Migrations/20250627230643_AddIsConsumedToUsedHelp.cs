using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IQGame.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsConsumedToUsedHelp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsConsumed",
                table: "UsedHelps",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsConsumed",
                table: "UsedHelps");
        }
    }
}
