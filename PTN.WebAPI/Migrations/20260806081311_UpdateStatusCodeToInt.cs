using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTN.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStatusCodeToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "status_code",
                table: "request_logs",
                type: "integer USING status_code ::integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status_code",
                table: "request_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
