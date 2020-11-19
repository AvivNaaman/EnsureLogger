using Microsoft.EntityFrameworkCore.Migrations;

namespace Ensure.Web.Migrations
{
    public partial class DailyTarget : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "TimeZone",
                table: "AspNetUsers",
                type: "smallint",
                nullable: false,
                defaultValue: (short)2,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<short>(
                name: "DailyTarget",
                table: "AspNetUsers",
                type: "smallint",
                nullable: false,
                defaultValue: (short)2);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyTarget",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<short>(
                name: "TimeZone",
                table: "AspNetUsers",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldDefaultValue: (short)2);
        }
    }
}
