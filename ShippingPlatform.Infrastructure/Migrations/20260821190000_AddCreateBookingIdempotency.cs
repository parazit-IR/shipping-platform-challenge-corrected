using System;
using Microsoft.EntityFrameworkCore.Migrations;
using ShippingPlatform.Infrastructure.Booking;

#nullable disable

namespace ShippingPlatform.Infrastructure.Migrations
{
    public partial class AddCreateBookingIdempotency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: CreateBookingIdempotencySchema.TableName,
                schema: CreateBookingIdempotencySchema.SchemaName,
                columns: table => new
                {
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    request_fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    state = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: true),
                    booking_status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(CreateBookingIdempotencySchema.PrimaryKeyName, x => x.idempotency_key);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: CreateBookingIdempotencySchema.TableName,
                schema: CreateBookingIdempotencySchema.SchemaName);
        }
    }
}
