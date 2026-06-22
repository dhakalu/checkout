using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBrandTableWebsiteUrlColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WebsiteUrl",
                table: "brands",
                newName: "website_url");

            migrationBuilder.AlterColumn<string>(
                name: "website_url",
                table: "brands",
                type: "varchar(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "website_url",
                table: "brands",
                newName: "WebsiteUrl");

            migrationBuilder.AlterColumn<string>(
                name: "WebsiteUrl",
                table: "brands",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)");
        }
    }
}