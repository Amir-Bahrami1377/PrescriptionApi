using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prescription.Modules.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovePriceFromLabTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceInRials",
                schema: "catalog",
                table: "lab_tests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PriceInRials",
                schema: "catalog",
                table: "lab_tests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
