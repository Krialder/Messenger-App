using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Index für schnelle Email-Suche (Login)
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_IsActive",
                table: "users",
                columns: new[] { "email", "is_active" });

            // Index für MFA-Methoden
            migrationBuilder.CreateIndex(
                name: "IX_MfaMethods_UserId_IsActive_MethodType",
                table: "mfa_methods",
                columns: new[] { "user_id", "is_active", "method_type" });

            // Index für Recovery Codes
            migrationBuilder.CreateIndex(
                name: "IX_RecoveryCodes_UserId_Used",
                table: "recovery_codes",
                columns: new[] { "user_id", "used" });

            // Index für Refresh Tokens
            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token_IsRevoked_ExpiresAt",
                table: "refresh_tokens",
                columns: new[] { "token", "is_revoked", "expires_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Users_Email_IsActive", table: "users");
            migrationBuilder.DropIndex(name: "IX_MfaMethods_UserId_IsActive_MethodType", table: "mfa_methods");
            migrationBuilder.DropIndex(name: "IX_RecoveryCodes_UserId_Used", table: "recovery_codes");
            migrationBuilder.DropIndex(name: "IX_RefreshTokens_Token_IsRevoked_ExpiresAt", table: "refresh_tokens");
        }
    }
}
