using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portal.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHrConsultant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create HrConsultants table
            migrationBuilder.CreateTable(
                name: "HrConsultants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrConsultants", x => x.Id);
                });

            // Create unique index on Email
            migrationBuilder.CreateIndex(
                name: "IX_HrConsultants_Email",
                table: "HrConsultants",
                column: "Email",
                unique: true);

            // Create HrConsultantTenantAssignments table
            migrationBuilder.CreateTable(
                name: "HrConsultantTenantAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HrConsultantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageRequestTypes = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageSettings = table.Column<bool>(type: "boolean", nullable: false),
                    CanManageBranding = table.Column<bool>(type: "boolean", nullable: false),
                    CanViewResponses = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrConsultantTenantAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrConsultantTenantAssignments_HrConsultants_HrConsultantId",
                        column: x => x.HrConsultantId,
                        principalTable: "HrConsultants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HrConsultantTenantAssignments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create unique index on HrConsultantId + TenantId
            migrationBuilder.CreateIndex(
                name: "IX_HrConsultantTenantAssignments_HrConsultantId_TenantId",
                table: "HrConsultantTenantAssignments",
                columns: new[] { "HrConsultantId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrConsultantTenantAssignments_TenantId",
                table: "HrConsultantTenantAssignments",
                column: "TenantId");

            // Alter RefreshTokens table - make UserId nullable and add HrConsultantId
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "RefreshTokens",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "HrConsultantId",
                table: "RefreshTokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_HrConsultantId",
                table: "RefreshTokens",
                column: "HrConsultantId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_HrConsultants_HrConsultantId",
                table: "RefreshTokens",
                column: "HrConsultantId",
                principalTable: "HrConsultants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove foreign key from RefreshTokens
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_HrConsultants_HrConsultantId",
                table: "RefreshTokens");

            // Remove index
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_HrConsultantId",
                table: "RefreshTokens");

            // Remove HrConsultantId column
            migrationBuilder.DropColumn(
                name: "HrConsultantId",
                table: "RefreshTokens");

            // Make UserId non-nullable again
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "RefreshTokens",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // Drop HrConsultantTenantAssignments table
            migrationBuilder.DropTable(
                name: "HrConsultantTenantAssignments");

            // Drop HrConsultants table
            migrationBuilder.DropTable(
                name: "HrConsultants");
        }
    }
}
