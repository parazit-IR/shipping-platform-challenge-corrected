using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShippingPlatform.Commercial.Infrastructure.Adapter.Out.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCommercial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "commercial");

            migrationBuilder.CreateTable(
                name: "customers",
                schema: "commercial",
                columns: table => new
                {
                    customer_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.customer_id);
                });

            migrationBuilder.CreateTable(
                name: "agreements",
                schema: "commercial",
                columns: table => new
                {
                    agreement_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    customer_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agreements", x => x.agreement_id);
                    table.ForeignKey(
                        name: "FK_agreements_customers_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "commercial",
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agreements_customer_id",
                schema: "commercial",
                table: "agreements",
                column: "customer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agreements",
                schema: "commercial");

            migrationBuilder.DropTable(
                name: "customers",
                schema: "commercial");
        }
    }
}
