using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddScopeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "scopes",
                columns: table => new
                {
                    key = table.Column<string>(type: "varchar(50)", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scopes", x => x.key);
                });

            migrationBuilder.CreateIndex(
                name: "IX_scopes_key",
                table: "scopes",
                column: "key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "scopes");
        }
    }
}
