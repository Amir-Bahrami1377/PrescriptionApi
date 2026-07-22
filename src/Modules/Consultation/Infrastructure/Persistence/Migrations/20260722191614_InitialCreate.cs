using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prescription.Modules.Consultation.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "consultation");

            migrationBuilder.CreateTable(
                name: "consultation_requests",
                schema: "consultation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageFileKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CustomerNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorOpinion = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    AnsweredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consultation_requests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_consultation_requests_CustomerId",
                schema: "consultation",
                table: "consultation_requests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_consultation_requests_Status",
                schema: "consultation",
                table: "consultation_requests",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consultation_requests",
                schema: "consultation");
        }
    }
}
