using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prescription.Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationFeeToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ConsultationFeeInRials",
                schema: "identity",
                table: "users",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsultationFeeInRials",
                schema: "identity",
                table: "users");
        }
    }
}
