using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prescription.Modules.Ticketing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketClosureQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AutoCloseAtUtc",
                schema: "ticketing",
                table: "tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "QueuedForClosureAtUtc",
                schema: "ticketing",
                table: "tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tickets_Status_AutoCloseAtUtc",
                schema: "ticketing",
                table: "tickets",
                columns: new[] { "Status", "AutoCloseAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tickets_Status_AutoCloseAtUtc",
                schema: "ticketing",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "AutoCloseAtUtc",
                schema: "ticketing",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "QueuedForClosureAtUtc",
                schema: "ticketing",
                table: "tickets");
        }
    }
}
