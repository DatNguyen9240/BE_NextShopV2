using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextShopV2.Infrastructure.Persistence.Migrations
{
    public partial class InitialBaseline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Baseline migration - database already contains current schema. No Up actions.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Baseline migration - no down actions.
        }
    }
}
