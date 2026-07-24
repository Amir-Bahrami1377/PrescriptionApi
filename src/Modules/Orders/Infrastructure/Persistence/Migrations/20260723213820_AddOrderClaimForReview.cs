using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prescription.Modules.Orders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderClaimForReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClaimExpiresAtUtc",
                schema: "orders",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimedByDoctorId",
                schema: "orders",
                table: "orders",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClaimExpiresAtUtc",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "ClaimedByDoctorId",
                schema: "orders",
                table: "orders");
        }
    }
}
