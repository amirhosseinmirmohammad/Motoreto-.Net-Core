using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdNeighborhoodAndCoords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "classified_ads",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "classified_ads",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "neighborhood",
                table: "classified_ads",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitude",
                table: "classified_ads");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "classified_ads");

            migrationBuilder.DropColumn(
                name: "neighborhood",
                table: "classified_ads");
        }
    }
}
