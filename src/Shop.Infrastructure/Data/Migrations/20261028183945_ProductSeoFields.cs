using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProductSeoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "meta_description",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "meta_title",
                table: "products",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "meta_description",
                table: "products");

            migrationBuilder.DropColumn(
                name: "meta_title",
                table: "products");
        }
    }
}
