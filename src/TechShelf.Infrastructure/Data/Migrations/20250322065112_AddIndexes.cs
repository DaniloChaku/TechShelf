using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechShelf.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Product_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Price",
                table: "Products",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_Order_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Email",
                table: "Orders",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Order_PaymentIntentId",
                table: "Orders",
                column: "PaymentIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_PhoneNumber",
                table: "Orders",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Total",
                table: "Orders",
                column: "Total");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Product_Name",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Product_Price",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Order_CustomerId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Order_Email",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Order_PaymentIntentId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Order_PhoneNumber",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Order_Total",
                table: "Orders");
        }
    }
}
