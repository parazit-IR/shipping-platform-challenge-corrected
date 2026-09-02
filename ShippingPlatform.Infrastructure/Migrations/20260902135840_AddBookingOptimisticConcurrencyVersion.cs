using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShippingPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingOptimisticConcurrencyVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "version",
                schema: "booking",
                table: "bookings",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "version",
                schema: "booking",
                table: "bookings");
        }
    }
}
