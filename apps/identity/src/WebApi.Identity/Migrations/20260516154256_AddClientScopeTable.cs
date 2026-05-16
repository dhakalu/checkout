using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddClientScopeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "client_scopes",
                columns: table => new
                {
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope_key = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_scopes", x => new { x.client_id, x.scope_key });
                    table.ForeignKey(
                        name: "FK_client_scopes_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_client_scopes_scopes_scope_key",
                        column: x => x.scope_key,
                        principalTable: "scopes",
                        principalColumn: "key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_client_scopes_client_id",
                table: "client_scopes",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_scopes_scope_key",
                table: "client_scopes",
                column: "scope_key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_scopes");
        }
    }
}
