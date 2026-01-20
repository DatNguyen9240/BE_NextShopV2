using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextShopV2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // All columns already exist in DB - added manually or in previous migrations
            // migrationBuilder.AddColumn<string>(
            //     name: "DeliveryAddress",
            //     table: "Shipments",
            //     type: "nvarchar(max)",
            //     nullable: true);

            // migrationBuilder.AddColumn<double>(
            //     name: "DeliveryLat",
            //     table: "Shipments",
            //     type: "float",
            //     nullable: true);

            // migrationBuilder.AddColumn<double>(
            //     name: "DeliveryLng",
            //     table: "Shipments",
            //     type: "float",
            //     nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ShippingLat",
                table: "Orders",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ShippingLng",
                table: "Orders",
                type: "float",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShipperId",
                table: "Shipments",
                column: "ShipperId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Users_ShipperId",
                table: "Shipments",
                column: "ShipperId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Users_ShipperId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_ShipperId",
                table: "Shipments");

            // Columns already exist, don't drop them
            // migrationBuilder.DropColumn(
            //     name: "DeliveryAddress",
            //     table: "Shipments");

            // migrationBuilder.DropColumn(
            //     name: "DeliveryLat",
            //     table: "Shipments");

            // migrationBuilder.DropColumn(
            //     name: "DeliveryLng",
            //     table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShippingLat",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingLng",
                table: "Orders");
        }
    }
}
