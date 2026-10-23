using CampusCart.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusCart.Api.Migrations;

[DbContext(typeof(CampusCartDbContext))]
[Migration("20261023120000_AddListingSoftDelete")]
public sealed class AddListingSoftDelete : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "service_listings",
            type: "boolean",
            nullable: false,
            defaultValue: false);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "service_listings");
}
