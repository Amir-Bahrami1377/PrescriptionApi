using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prescription.Modules.Orders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionRenewals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prescription_renewals",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentPrescriptionReferenceNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NationalCode = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BasicInsurance = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PriceInRials = table.Column<long>(type: "bigint", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ClaimedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PaymentAuthority = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NewPrescriptionReferenceNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescription_renewals", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_prescription_renewals_CustomerId",
                schema: "orders",
                table: "prescription_renewals",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_prescription_renewals_Status",
                schema: "orders",
                table: "prescription_renewals",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prescription_renewals",
                schema: "orders");
        }
    }
}
