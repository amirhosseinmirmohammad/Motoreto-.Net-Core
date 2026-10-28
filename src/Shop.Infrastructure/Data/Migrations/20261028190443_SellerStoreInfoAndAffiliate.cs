using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shop.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SellerStoreInfoAndAffiliate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "sellers",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "sellers",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_methods",
                table: "sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "affiliate_code",
                table: "orders",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "affiliates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    commission_percent = table.Column<int>(type: "integer", nullable: false),
                    click_count = table.Column<int>(type: "integer", nullable: false),
                    order_count = table.Column<int>(type: "integer", nullable: false),
                    order_amount = table.Column<long>(type: "bigint", nullable: false),
                    earned = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_affiliates", x => x.id);
                    table.ForeignKey(
                        name: "fk_affiliates_user_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_affiliates_code",
                table: "affiliates",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_affiliates_user_id",
                table: "affiliates",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "affiliates");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "sellers");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "sellers");

            migrationBuilder.DropColumn(
                name: "payment_methods",
                table: "sellers");

            migrationBuilder.DropColumn(
                name: "affiliate_code",
                table: "orders");
        }
    }
}
