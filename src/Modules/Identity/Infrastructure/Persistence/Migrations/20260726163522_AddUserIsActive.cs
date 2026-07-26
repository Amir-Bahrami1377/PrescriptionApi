using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prescription.Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // defaultValue MUST be true: EF scaffolds `false` for bools, which would flag every
            // pre-existing user as deleted and lock them (including the admin) out of the system.
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "identity",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "identity",
                table: "users");
        }
    }
}
