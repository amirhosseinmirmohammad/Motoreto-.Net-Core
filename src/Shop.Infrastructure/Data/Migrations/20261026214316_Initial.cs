using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shop.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "carts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    guest_token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_carts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_categories_categories_parent_id",
                        column: x => x.parent_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "discounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    percent = table.Column<byte>(type: "smallint", nullable: false),
                    max_amount = table.Column<long>(type: "bigint", nullable: true),
                    min_order_amount = table.Column<long>(type: "bigint", nullable: false),
                    usage_limit = table.Column<int>(type: "integer", nullable: true),
                    used_count = table.Column<int>(type: "integer", nullable: false),
                    expire_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identity_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "news_letter_subscribers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    subscribed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_news_letter_subscribers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "site_messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    subject = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_site_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "site_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    store_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tagline = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    logo_url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    primary_color = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    support_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    address = table.Column<string>(type: "text", nullable: true),
                    instagram_url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    telegram_url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    about_us = table.Column<string>(type: "text", nullable: true),
                    contact_us = table.Column<string>(type: "text", nullable: true),
                    shipping_post_cost = table.Column<long>(type: "bigint", nullable: false),
                    shipping_express_cost = table.Column<long>(type: "bigint", nullable: false),
                    free_shipping_threshold = table.Column<long>(type: "bigint", nullable: false),
                    reservation_minutes = table.Column<int>(type: "integer", nullable: false),
                    max_quantity_per_product = table.Column<int>(type: "integer", nullable: false),
                    low_stock_threshold = table.Column<int>(type: "integer", nullable: false),
                    reservation_cleanup_interval_seconds = table.Column<int>(type: "integer", nullable: false),
                    payment_gateway = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_maintenance_mode = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_site_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sliders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    sub_title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    image = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    link = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    theme = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sliders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "states",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    english_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    unit_price = table.Column<long>(type: "bigint", nullable: false),
                    discount_percent = table.Column<byte>(type: "smallint", nullable: false),
                    stock = table.Column<int>(type: "integer", nullable: false),
                    reserved_count = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    main_image = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    is_amazing = table.Column<bool>(type: "boolean", nullable: false),
                    amazing_ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_special = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    sales_count = table.Column<int>(type: "integer", nullable: false),
                    view_count = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "identity_role_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_role_claims_identity_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "identity_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cities", x => x.id);
                    table.ForeignKey(
                        name: "fk_cities_states_state_id",
                        column: x => x.state_id,
                        principalTable: "states",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cart_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cart_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_cart_items_carts_cart_id",
                        column: x => x.cart_id,
                        principalTable: "carts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cart_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    alt = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_images_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    mobile = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    state_id = table.Column<int>(type: "integer", nullable: true),
                    city_id = table.Column<int>(type: "integer", nullable: true),
                    address_line = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_users_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_identity_users_states_state_id",
                        column: x => x.state_id,
                        principalTable: "states",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "blogs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    short_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    cover_image = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false),
                    view_count = table.Column<long>(type: "bigint", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blogs", x => x.id);
                    table.ForeignKey(
                        name: "fk_blogs_user_author_id",
                        column: x => x.author_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<byte>(type: "smallint", nullable: false),
                    text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    is_approved = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_comments_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_comments_user_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "favorites",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favorites", x => x.id);
                    table.ForeignKey(
                        name: "fk_favorites_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_favorites_user_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_user_claims_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_identity_user_logins_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_roles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_identity_user_roles_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_identity_user_roles_identity_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "identity_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_tokens",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_identity_user_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    payment_method = table.Column<int>(type: "integer", nullable: false),
                    shipping_method = table.Column<int>(type: "integer", nullable: false),
                    subtotal = table.Column<long>(type: "bigint", nullable: false),
                    discount_amount = table.Column<long>(type: "bigint", nullable: false),
                    shipping_cost = table.Column<long>(type: "bigint", nullable: false),
                    total = table.Column<long>(type: "bigint", nullable: false),
                    discount_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    receiver_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    state_id = table.Column<int>(type: "integer", nullable: false),
                    city_id = table.Column<int>(type: "integer", nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cancel_reason = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    shipped_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_orders_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orders_states_state_id",
                        column: x => x.state_id,
                        principalTable: "states",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orders_user_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "blog_comments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    blog_id = table.Column<int>(type: "integer", nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    is_approved = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_blog_comments_blogs_blog_id",
                        column: x => x.blog_id,
                        principalTable: "blogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    product_name = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    product_image = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    unit_price = table.Column<long>(type: "bigint", nullable: false),
                    discount_percent = table.Column<byte>(type: "smallint", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    gateway = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    authority = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ref_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_payments_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_reservations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<int>(type: "integer", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    released_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_reservations", x => x.id);
                    table.ForeignKey(
                        name: "fk_stock_reservations_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_stock_reservations_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_stock_reservations_user_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_blog_comments_blog_id_is_approved",
                table: "blog_comments",
                columns: new[] { "blog_id", "is_approved" });

            migrationBuilder.CreateIndex(
                name: "ix_blogs_author_id",
                table: "blogs",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_blogs_is_visible_created_at",
                table: "blogs",
                columns: new[] { "is_visible", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_blogs_slug",
                table: "blogs",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_cart_id_product_id",
                table: "cart_items",
                columns: new[] { "cart_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_product_id",
                table: "cart_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_carts_guest_token",
                table: "carts",
                column: "guest_token");

            migrationBuilder.CreateIndex(
                name: "ix_carts_user_id",
                table: "carts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_id",
                table: "categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_slug",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cities_state_id",
                table: "cities",
                column: "state_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_product_id_is_approved",
                table: "comments",
                columns: new[] { "product_id", "is_approved" });

            migrationBuilder.CreateIndex(
                name: "ix_comments_user_id",
                table: "comments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_discounts_code",
                table: "discounts",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_favorites_product_id",
                table: "favorites",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_favorites_user_id_product_id",
                table: "favorites",
                columns: new[] { "user_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identity_role_claims_role_id",
                table: "identity_role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "identity_roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_claims_user_id",
                table: "identity_user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_logins_user_id",
                table: "identity_user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_roles_role_id",
                table: "identity_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "identity_users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "ix_identity_users_city_id",
                table: "identity_users",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_users_mobile",
                table: "identity_users",
                column: "mobile");

            migrationBuilder.CreateIndex(
                name: "ix_identity_users_state_id",
                table: "identity_users",
                column: "state_id");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "identity_users",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_news_letter_subscribers_email",
                table: "news_letter_subscribers",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_product_id",
                table: "order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_city_id",
                table: "orders",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_created_at",
                table: "orders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_orders_order_number",
                table: "orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_state_id",
                table: "orders",
                column: "state_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_status",
                table: "orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_orders_user_id_created_at",
                table: "orders",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_authority",
                table: "payments",
                column: "authority",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payments_order_id",
                table: "payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_images_product_id_sort_order",
                table: "product_images",
                columns: new[] { "product_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id_is_active",
                table: "products",
                columns: new[] { "category_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_products_code",
                table: "products",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_created_at",
                table: "products",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_products_is_active_is_amazing",
                table: "products",
                columns: new[] { "is_active", "is_amazing" });

            migrationBuilder.CreateIndex(
                name: "ix_products_is_active_is_special",
                table: "products",
                columns: new[] { "is_active", "is_special" });

            migrationBuilder.CreateIndex(
                name: "ix_products_sales_count",
                table: "products",
                column: "sales_count");

            migrationBuilder.CreateIndex(
                name: "ix_products_slug",
                table: "products",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_unit_price",
                table: "products",
                column: "unit_price");

            migrationBuilder.CreateIndex(
                name: "ix_site_messages_is_read_created_at",
                table: "site_messages",
                columns: new[] { "is_read", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_sliders_is_active_sort_order",
                table: "sliders",
                columns: new[] { "is_active", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_states_name",
                table: "states",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_reservations_order_id",
                table: "stock_reservations",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_reservations_product_id",
                table: "stock_reservations",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_reservations_status_expires_at",
                table: "stock_reservations",
                columns: new[] { "status", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_reservations_user_id_status",
                table: "stock_reservations",
                columns: new[] { "user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blog_comments");

            migrationBuilder.DropTable(
                name: "cart_items");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "discounts");

            migrationBuilder.DropTable(
                name: "favorites");

            migrationBuilder.DropTable(
                name: "identity_role_claims");

            migrationBuilder.DropTable(
                name: "identity_user_claims");

            migrationBuilder.DropTable(
                name: "identity_user_logins");

            migrationBuilder.DropTable(
                name: "identity_user_roles");

            migrationBuilder.DropTable(
                name: "identity_user_tokens");

            migrationBuilder.DropTable(
                name: "news_letter_subscribers");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "product_images");

            migrationBuilder.DropTable(
                name: "site_messages");

            migrationBuilder.DropTable(
                name: "site_settings");

            migrationBuilder.DropTable(
                name: "sliders");

            migrationBuilder.DropTable(
                name: "stock_reservations");

            migrationBuilder.DropTable(
                name: "blogs");

            migrationBuilder.DropTable(
                name: "carts");

            migrationBuilder.DropTable(
                name: "identity_roles");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "identity_users");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "cities");

            migrationBuilder.DropTable(
                name: "states");
        }
    }
}
