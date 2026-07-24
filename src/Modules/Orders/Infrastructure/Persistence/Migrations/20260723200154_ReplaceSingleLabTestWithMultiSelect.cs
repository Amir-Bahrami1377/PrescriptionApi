using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prescription.Modules.Orders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceSingleLabTestWithMultiSelect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LabTestId",
                schema: "orders",
                table: "orders");

            migrationBuilder.AddColumn<Guid[]>(
                name: "lab_test_ids",
                schema: "orders",
                table: "orders",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lab_test_ids",
                schema: "orders",
                table: "orders");

            migrationBuilder.AddColumn<Guid>(
                name: "LabTestId",
                schema: "orders",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
