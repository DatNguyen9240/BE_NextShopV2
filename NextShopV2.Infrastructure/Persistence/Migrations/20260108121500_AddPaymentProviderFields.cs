using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextShopV2.Infrastructure.Persistence.Migrations
{
    public partial class AddPaymentProviderFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderPaymentId",
                table: "Payments",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderData",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProviderPaymentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ProviderData",
                table: "Payments");
        }
    }
}
