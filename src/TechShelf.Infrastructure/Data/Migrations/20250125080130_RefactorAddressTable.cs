using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechShelf.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorAddressTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Address_Region",
                table: "Orders",
                newName: "Address_State");

            migrationBuilder.RenameColumn(
                name: "Address_AddressLine2",
                table: "Orders",
                newName: "Address_Line2");

            migrationBuilder.RenameColumn(
                name: "Address_AddressLine1",
                table: "Orders",
                newName: "Address_Line1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address_State",
                table: "Orders",
                newName: "Address_Region");

            migrationBuilder.RenameColumn(
                name: "Address_Line2",
                table: "Orders",
                newName: "Address_AddressLine2");

            migrationBuilder.RenameColumn(
                name: "Address_Line1",
                table: "Orders",
                newName: "Address_AddressLine1");

            migrationBuilder.AddColumn<string>(
                name: "Address_Country",
                table: "Orders",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");
        }
    }
}
