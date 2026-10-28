using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shop.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AffiliateCpaCpl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "lead_count",
                table: "affiliates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "paid_out",
                table: "affiliates",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "payout_requested_at",
                table: "affiliates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "registration_count",
                table: "affiliates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "affiliate_leads",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    affiliate_id = table.Column<int>(type: "integer", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    interest = table.Column<string>(type: "text", nullable: true),
                    is_contacted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_affiliate_leads", x => x.id);
                    table.ForeignKey(
                        name: "fk_affiliate_leads_affiliates_affiliate_id",
                        column: x => x.affiliate_id,
                        principalTable: "affiliates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_affiliate_leads_affiliate_id",
                table: "affiliate_leads",
                column: "affiliate_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "affiliate_leads");

            migrationBuilder.DropColumn(
                name: "lead_count",
                table: "affiliates");

            migrationBuilder.DropColumn(
                name: "paid_out",
                table: "affiliates");

            migrationBuilder.DropColumn(
                name: "payout_requested_at",
                table: "affiliates");

            migrationBuilder.DropColumn(
                name: "registration_count",
                table: "affiliates");
        }
    }
}
