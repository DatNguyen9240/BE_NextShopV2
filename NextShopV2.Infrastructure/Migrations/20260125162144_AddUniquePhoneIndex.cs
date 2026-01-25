using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextShopV2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquePhoneIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlServer = migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer";

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Users",
                type: isSqlServer ? "nvarchar(450)" : "varchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: isSqlServer ? "nvarchar(max)" : "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Phone",
                table: "Users",
                column: "Phone",
                unique: true,
                filter: isSqlServer ? "[Phone] IS NOT NULL" : "\"Phone\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var isSqlServer = migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer";

            migrationBuilder.DropIndex(
                name: "IX_Users_Phone",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Users",
                type: isSqlServer ? "nvarchar(max)" : "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: isSqlServer ? "nvarchar(450)" : "varchar(450)",
                oldNullable: true);
        }
    }
}
