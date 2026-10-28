using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shop.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandsAndCategoryAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "brand_id",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "brands",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_brands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "category_attributes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    show_in_filter = table.Column<bool>(type: "boolean", nullable: false),
                    is_variant_option = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category_attributes", x => x.id);
                    table.ForeignKey(
                        name: "fk_category_attributes_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_products_brand_id",
                table: "products",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "ix_category_attributes_category_id",
                table: "category_attributes",
                column: "category_id");

            migrationBuilder.AddForeignKey(
                name: "fk_products_brands_brand_id",
                table: "products",
                column: "brand_id",
                principalTable: "brands",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_products_brands_brand_id",
                table: "products");

            migrationBuilder.DropTable(
                name: "brands");

            migrationBuilder.DropTable(
                name: "category_attributes");

            migrationBuilder.DropIndex(
                name: "ix_products_brand_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "brand_id",
                table: "products");
        }
    }
}
