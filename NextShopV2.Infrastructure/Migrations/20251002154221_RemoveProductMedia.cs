using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextShopV2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductMedia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductMedia",
                columns: table => new
                {
                    MediaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    MediaType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MediaUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMedia", x => x.MediaId);
                    table.ForeignKey(
                        name: "FK_ProductMedia_ProductVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "VariantId");
                    table.ForeignKey(
                        name: "FK_ProductMedia_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductMedia_ProductId",
                table: "ProductMedia",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMedia_VariantId",
                table: "ProductMedia",
                column: "VariantId");
        }
    }
}
