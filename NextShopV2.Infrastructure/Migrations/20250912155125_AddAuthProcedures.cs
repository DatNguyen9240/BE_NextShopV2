using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextShopV2.Infrastructure.Migrations
{
    public partial class AddAuthProcedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
@"
IF OBJECT_ID('dbo.RegisterUser', 'P') IS NOT NULL
    DROP PROCEDURE dbo.RegisterUser;
"
            );
            migrationBuilder.Sql(
@"
CREATE PROCEDURE dbo.RegisterUser
    @Email NVARCHAR(256),
    @PasswordHash NVARCHAR(256)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
    BEGIN
        RAISERROR('User already exists', 16, 1);
        RETURN;
    END
    INSERT INTO Users (Id, Email, PasswordHash, Role, CreatedAt)
    VALUES (NEWID(), @Email, @PasswordHash, 'User', GETDATE());
END
"
            );

            migrationBuilder.Sql(
@"
IF OBJECT_ID('dbo.CheckUserLogin', 'P') IS NOT NULL
    DROP PROCEDURE dbo.CheckUserLogin;
"
            );
            migrationBuilder.Sql(
@"
CREATE PROCEDURE dbo.CheckUserLogin
    @Email NVARCHAR(256),
    @PasswordHash NVARCHAR(256)
AS
BEGIN
    SELECT * FROM Users WHERE Email = @Email AND PasswordHash = @PasswordHash;
END
"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.RegisterUser;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.CheckUserLogin;");
        }
    }
}
