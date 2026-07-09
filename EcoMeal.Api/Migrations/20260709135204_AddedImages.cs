using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoMeal.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PackageImageUrl",
                table: "Package",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessImageUrl",
                table: "Business",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackageImageUrl",
                table: "Package");

            migrationBuilder.DropColumn(
                name: "BusinessImageUrl",
                table: "Business");
        }
    }
}
