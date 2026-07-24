using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prescription.Modules.Orders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInsuranceAndThirdPartyBeneficiary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BasicInsurance",
                schema: "orders",
                table: "orders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<bool>(
                name: "IsForThirdParty",
                schema: "orders",
                table: "orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SupplementaryInsurance",
                schema: "orders",
                table: "orders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<string>(
                name: "ThirdPartyNationalCode",
                schema: "orders",
                table: "orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThirdPartyPhoneNumber",
                schema: "orders",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasicInsurance",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "IsForThirdParty",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "SupplementaryInsurance",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "ThirdPartyNationalCode",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "ThirdPartyPhoneNumber",
                schema: "orders",
                table: "orders");
        }
    }
}
