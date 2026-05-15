using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsIdentityTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "identities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "is_email_verified",
                table: "identities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_locked",
                table: "identities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_mfa_endabled",
                table: "identities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "identities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "identities");

            migrationBuilder.DropColumn(
                name: "is_email_verified",
                table: "identities");

            migrationBuilder.DropColumn(
                name: "is_locked",
                table: "identities");

            migrationBuilder.DropColumn(
                name: "is_mfa_endabled",
                table: "identities");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "identities");
        }
    }
}
