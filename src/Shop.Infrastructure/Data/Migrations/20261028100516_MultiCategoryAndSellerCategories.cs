using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MultiCategoryAndSellerCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_categories",
                columns: table => new
                {
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_categories", x => new { x.product_id, x.category_id });
                    table.ForeignKey(
                        name: "fk_product_categories_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_categories_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seller_categories",
                columns: table => new
                {
                    seller_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_seller_categories", x => new { x.seller_id, x.category_id });
                    table.ForeignKey(
                        name: "fk_seller_categories_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_seller_categories_sellers_seller_id",
                        column: x => x.seller_id,
                        principalTable: "sellers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_categories_category_id",
                table: "product_categories",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_seller_categories_category_id",
                table: "seller_categories",
                column: "category_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_categories");

            migrationBuilder.DropTable(
                name: "seller_categories");
        }
    }
}
