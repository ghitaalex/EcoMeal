using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoMeal.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStripePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Order",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Paid");

            migrationBuilder.AddColumn<bool>(
                name: "StockReserved",
                table: "Order",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Order",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE [o]
                SET [o].[TotalAmount] = [p].[Price],
                    [o].[StockReserved] = CASE
                        WHEN LOWER([o].[Status]) IN ('cancelled', 'canceled') THEN 0
                        ELSE 1
                    END
                FROM [Order] AS [o]
                INNER JOIN [Package] AS [p] ON [o].[PackageId] = [p].[Id];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "StockReserved",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Order");
        }
    }
}
