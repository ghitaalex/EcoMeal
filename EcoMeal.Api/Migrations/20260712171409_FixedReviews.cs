using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoMeal.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixedReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_AspNetUsers_UserId",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Business_BusinessId",
                table: "Review");

            migrationBuilder.DropIndex(
                name: "IX_Review_BusinessId",
                table: "Review");

            migrationBuilder.RenameColumn(
                name: "BusinessId",
                table: "Review",
                newName: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Review_OrderId",
                table: "Review",
                column: "OrderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_AspNetUsers_UserId",
                table: "Review",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Order_OrderId",
                table: "Review",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_AspNetUsers_UserId",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Order_OrderId",
                table: "Review");

            migrationBuilder.DropIndex(
                name: "IX_Review_OrderId",
                table: "Review");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "Review",
                newName: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Review_BusinessId",
                table: "Review",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_AspNetUsers_UserId",
                table: "Review",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Business_BusinessId",
                table: "Review",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
