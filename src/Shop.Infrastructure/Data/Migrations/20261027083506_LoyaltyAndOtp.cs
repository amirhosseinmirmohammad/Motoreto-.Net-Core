using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shop.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class LoyaltyAndOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "points",
                table: "identity_users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "referral_code",
                table: "identity_users",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "referred_by_user_id",
                table: "identity_users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "wallet_balance",
                table: "identity_users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "otp_codes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mobile = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    failed_attempts = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_otp_codes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wallet_transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    balance_after = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_wallet_transactions_user_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transactions_user_id",
                table: "wallet_transactions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "otp_codes");

            migrationBuilder.DropTable(
                name: "wallet_transactions");

            migrationBuilder.DropColumn(
                name: "points",
                table: "identity_users");

            migrationBuilder.DropColumn(
                name: "referral_code",
                table: "identity_users");

            migrationBuilder.DropColumn(
                name: "referred_by_user_id",
                table: "identity_users");

            migrationBuilder.DropColumn(
                name: "wallet_balance",
                table: "identity_users");
        }
    }
}
