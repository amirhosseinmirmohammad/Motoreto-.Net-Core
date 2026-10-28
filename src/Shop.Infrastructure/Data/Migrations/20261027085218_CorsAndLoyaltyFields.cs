using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CorsAndLoyaltyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "additional_cors_origins",
                table: "site_settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "frontend_url",
                table: "site_settings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "additional_cors_origins",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "frontend_url",
                table: "site_settings");
        }
    }
}
