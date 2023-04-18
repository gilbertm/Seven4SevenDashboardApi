using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrators.PostgreSQL.Migrations.Tenant
{
    public partial class PostgreSQL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ValidUpto",
                schema: "MultiTenancy",
                table: "Tenants",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ValidUpto",
                schema: "MultiTenancy",
                table: "Tenants",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
