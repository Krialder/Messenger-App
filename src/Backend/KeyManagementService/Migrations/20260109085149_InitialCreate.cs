using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KeyManagementService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "public_keys",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    public_key = table.Column<byte[]>(type: "bytea", maxLength: 32, nullable: false),
                    algorithm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "X25519"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() + INTERVAL '1 year'"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_public_keys", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_public_keys_expires_at",
                table: "public_keys",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_public_keys_user_active",
                table: "public_keys",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "idx_public_keys_user_id",
                table: "public_keys",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "public_keys");
        }
    }
}
