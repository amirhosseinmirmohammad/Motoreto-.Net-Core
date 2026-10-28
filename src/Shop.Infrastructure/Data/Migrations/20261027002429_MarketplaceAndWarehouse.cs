using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shop.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MarketplaceAndWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_cart_items_cart_id_product_id",
                table: "cart_items");

            migrationBuilder.AddColumn<long>(
                name: "offer_id",
                table: "stock_reservations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "variant_id",
                table: "stock_reservations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_merchant_id",
                table: "site_settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "payment_sandbox",
                table: "site_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "tax_percent",
                table: "site_settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "attributes_json",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "tax_amount",
                table: "orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "offer_id",
                table: "order_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "seller_id",
                table: "order_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "seller_name",
                table: "order_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "variant_id",
                table: "order_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "variant_text",
                table: "order_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "offer_id",
                table: "cart_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "variant_id",
                table: "cart_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "price_histories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    price = table.Column<long>(type: "bigint", nullable: false),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_price_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_price_histories_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_variants",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    attributes_json = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    price = table.Column<long>(type: "bigint", nullable: true),
                    discount_percent = table.Column<byte>(type: "smallint", nullable: false),
                    stock = table.Column<int>(type: "integer", nullable: false),
                    reserved_count = table.Column<int>(type: "integer", nullable: false),
                    image = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_variants", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_variants_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sellers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    store_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(170)", maxLength: 170, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    address = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    sheba = table.Column<string>(type: "text", nullable: true),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    national_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    company_name = table.Column<string>(type: "text", nullable: true),
                    company_national_id = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    economic_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    registration_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ceo_name = table.Column<string>(type: "text", nullable: true),
                    ceo_national_id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    business_license_number = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    business_license_file_url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    national_card_file_url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    company_ad_file_url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    shipping_title = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    delivery_days = table.Column<int>(type: "integer", nullable: false),
                    delivery_days_max = table.Column<int>(type: "integer", nullable: false),
                    shipping_cost_override = table.Column<long>(type: "bigint", nullable: true),
                    free_shipping_threshold_override = table.Column<long>(type: "bigint", nullable: true),
                    tax_percent = table.Column<int>(type: "integer", nullable: false),
                    service_fee = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    reject_reason = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    commission_percent = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sellers", x => x.id);
                    table.ForeignKey(
                        name: "fk_sellers_identity_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_movements",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    delta = table.Column<int>(type: "integer", nullable: false),
                    stock_after = table.Column<int>(type: "integer", nullable: false),
                    reference = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_movements", x => x.id);
                    table.ForeignKey(
                        name: "fk_stock_movements_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seller_offers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    seller_id = table.Column<long>(type: "bigint", nullable: false),
                    price = table.Column<long>(type: "bigint", nullable: false),
                    discount_percent = table.Column<byte>(type: "smallint", nullable: false),
                    stock = table.Column<int>(type: "integer", nullable: false),
                    reserved_count = table.Column<int>(type: "integer", nullable: false),
                    warranty = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    delivery_days = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_seller_offers", x => x.id);
                    table.ForeignKey(
                        name: "fk_seller_offers_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_seller_offers_sellers_seller_id",
                        column: x => x.seller_id,
                        principalTable: "sellers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_stock_reservations_offer_id",
                table: "stock_reservations",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_reservations_variant_id",
                table: "stock_reservations",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_seller_id",
                table: "order_items",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_cart_id_product_id_offer_id",
                table: "cart_items",
                columns: new[] { "cart_id", "product_id", "offer_id" });

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_offer_id",
                table: "cart_items",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_variant_id",
                table: "cart_items",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "ix_price_histories_product_id_recorded_at",
                table: "price_histories",
                columns: new[] { "product_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "ix_product_variants_product_id_is_active",
                table: "product_variants",
                columns: new[] { "product_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_seller_offers_is_active",
                table: "seller_offers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_seller_offers_product_id_seller_id",
                table: "seller_offers",
                columns: new[] { "product_id", "seller_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_seller_offers_seller_id",
                table: "seller_offers",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "ix_sellers_owner_user_id",
                table: "sellers",
                column: "owner_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sellers_slug",
                table: "sellers",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sellers_status",
                table: "sellers",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_product_id_created_at",
                table: "stock_movements",
                columns: new[] { "product_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_type",
                table: "stock_movements",
                column: "type");

            migrationBuilder.AddForeignKey(
                name: "fk_cart_items_product_variants_variant_id",
                table: "cart_items",
                column: "variant_id",
                principalTable: "product_variants",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_cart_items_seller_offers_offer_id",
                table: "cart_items",
                column: "offer_id",
                principalTable: "seller_offers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_stock_reservations_product_variants_variant_id",
                table: "stock_reservations",
                column: "variant_id",
                principalTable: "product_variants",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_stock_reservations_seller_offers_offer_id",
                table: "stock_reservations",
                column: "offer_id",
                principalTable: "seller_offers",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cart_items_product_variants_variant_id",
                table: "cart_items");

            migrationBuilder.DropForeignKey(
                name: "fk_cart_items_seller_offers_offer_id",
                table: "cart_items");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_reservations_product_variants_variant_id",
                table: "stock_reservations");

            migrationBuilder.DropForeignKey(
                name: "fk_stock_reservations_seller_offers_offer_id",
                table: "stock_reservations");

            migrationBuilder.DropTable(
                name: "price_histories");

            migrationBuilder.DropTable(
                name: "product_variants");

            migrationBuilder.DropTable(
                name: "seller_offers");

            migrationBuilder.DropTable(
                name: "stock_movements");

            migrationBuilder.DropTable(
                name: "sellers");

            migrationBuilder.DropIndex(
                name: "ix_stock_reservations_offer_id",
                table: "stock_reservations");

            migrationBuilder.DropIndex(
                name: "ix_stock_reservations_variant_id",
                table: "stock_reservations");

            migrationBuilder.DropIndex(
                name: "ix_order_items_seller_id",
                table: "order_items");

            migrationBuilder.DropIndex(
                name: "ix_cart_items_cart_id_product_id_offer_id",
                table: "cart_items");

            migrationBuilder.DropIndex(
                name: "ix_cart_items_offer_id",
                table: "cart_items");

            migrationBuilder.DropIndex(
                name: "ix_cart_items_variant_id",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "offer_id",
                table: "stock_reservations");

            migrationBuilder.DropColumn(
                name: "variant_id",
                table: "stock_reservations");

            migrationBuilder.DropColumn(
                name: "payment_merchant_id",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "payment_sandbox",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "tax_percent",
                table: "site_settings");

            migrationBuilder.DropColumn(
                name: "attributes_json",
                table: "products");

            migrationBuilder.DropColumn(
                name: "tax_amount",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "offer_id",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "seller_id",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "seller_name",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "variant_id",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "variant_text",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "offer_id",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "variant_id",
                table: "cart_items");

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_cart_id_product_id",
                table: "cart_items",
                columns: new[] { "cart_id", "product_id" },
                unique: true);
        }
    }
}
