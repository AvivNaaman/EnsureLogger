using Microsoft.EntityFrameworkCore.Migrations;

namespace Ensure.Web.Migrations
{
    public partial class DailyTarget : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TimeZone",
                table: "AspNetUsers",
                type: "smallint",
                nullable: false,
                defaultValue: (int)2,
                oldClrType: typeof(int),
                oldType: "smallint");

            migrationBuilder.AddColumn<int>(
                name: "DailyTarget",
                table: "AspNetUsers",
                type: "smallint",
                nullable: false,
                defaultValue: (int)2);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyTarget",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "TimeZone",
                table: "AspNetUsers",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "smallint",
                oldDefaultValue: (int)2);
        }
    }
}
