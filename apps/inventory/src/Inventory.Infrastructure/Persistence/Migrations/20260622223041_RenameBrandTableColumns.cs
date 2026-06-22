using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameBrandTableColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "brands",
                newName: "slug");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "brands",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "brands",
                newName: "is_active");

            migrationBuilder.AlterColumn<string>(
                name: "slug",
                table: "brands",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "brands",
                type: "varchar(1000)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "slug",
                table: "brands",
                newName: "Slug");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "brands",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "brands",
                newName: "IsActive");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "brands",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "brands",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)");
        }
    }
}