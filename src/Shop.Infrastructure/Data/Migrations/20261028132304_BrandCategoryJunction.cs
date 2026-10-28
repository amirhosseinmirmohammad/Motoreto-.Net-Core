using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class BrandCategoryJunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "brand_categories",
                columns: table => new
                {
                    brand_id = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_brand_categories", x => new { x.brand_id, x.category_id });
                    table.ForeignKey(
                        name: "fk_brand_categories_brands_brand_id",
                        column: x => x.brand_id,
                        principalTable: "brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_brand_categories_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_brand_categories_category_id",
                table: "brand_categories",
                column: "category_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "brand_categories");
        }
    }
}
