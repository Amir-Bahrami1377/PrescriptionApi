using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prescription.Modules.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLabTestDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Deliberately not 0. Rows already in the table are either seeded ones — which the
            // seeder renumbers to 1..n on the next startup — or tests an admin added, which belong
            // after the curated list. Defaulting to 0 would have floated those admin tests to the
            // very top instead.
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                schema: "catalog",
                table: "lab_tests",
                type: "integer",
                nullable: false,
                defaultValue: 1000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                schema: "catalog",
                table: "lab_tests");
        }
    }
}
