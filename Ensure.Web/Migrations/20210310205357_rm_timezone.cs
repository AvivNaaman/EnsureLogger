using Microsoft.EntityFrameworkCore.Migrations;

namespace Ensure.Web.Migrations
{
    public partial class rm_timezone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeZone",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 2);
        }
    }
}
