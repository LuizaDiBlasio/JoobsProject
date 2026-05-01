using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal_API.Migrations
{
    public partial class InitCreateLuiza : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "8fb7ed1d-14f5-44a7-a7bd-d4769a767665");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "467f8f83-430a-466d-a0db-6e4e01ce3500");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                column: "ConcurrencyStamp",
                value: "141c1756-17b4-498b-b5d6-063c6d3c8466");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "10",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b9994cb8-4040-4f75-ae0e-ce01dbfa94a0", "AQAAAAEAACcQAAAAENtOvkuC87kW59nwsKepHuZAL73hZefNc/Mo30GE5OygPIndmYyC145kQUMW5fPHmg==", "b8b5a162-478d-4fab-b5d0-86986056149a" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "ab43f7c0-c909-43c0-a6b4-98ff496bc0ca");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "d2f06762-dfc4-469b-9553-4e518e5c9d1f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                column: "ConcurrencyStamp",
                value: "5ef998b5-483c-43ce-8a16-4fe5b302a5ee");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "10",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8be122d1-6b64-4297-aab2-b3afc7afbcc0", "AQAAAAEAACcQAAAAENtpzGkzkBEdIP7QKPYs+SztY7iYO1EFX2YThEEM8Ai8XBerqdwoQkQb1DhOPBCCIw==", "f57ce49e-1b34-48b6-a9b5-248ae46c0efc" });
        }
    }
}
