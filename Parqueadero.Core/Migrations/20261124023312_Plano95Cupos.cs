using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Parqueadero.Core.Migrations
{
    /// <inheritdoc />
    public partial class Plano95Cupos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 76,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-01", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 77,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-02", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 78,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-03", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 79,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-04", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 80,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-05", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 81,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-06", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 82,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-07", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 83,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-08", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 84,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-09", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 85,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-10", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 86,
                column: "Codigo",
                value: "M-11");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 87,
                column: "Codigo",
                value: "M-12");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 88,
                column: "Codigo",
                value: "M-13");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 89,
                column: "Codigo",
                value: "M-14");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 90,
                column: "Codigo",
                value: "M-15");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 91,
                column: "Codigo",
                value: "M-16");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 92,
                column: "Codigo",
                value: "M-17");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 93,
                column: "Codigo",
                value: "M-18");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 94,
                column: "Codigo",
                value: "M-19");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 95,
                column: "Codigo",
                value: "M-20");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 76,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-76", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 77,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-77", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 78,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-78", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 79,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-79", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 80,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-80", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 81,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-81", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 82,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-82", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 83,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-83", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 84,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-84", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 85,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-85", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 86,
                column: "Codigo",
                value: "M-01");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 87,
                column: "Codigo",
                value: "M-02");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 88,
                column: "Codigo",
                value: "M-03");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 89,
                column: "Codigo",
                value: "M-04");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 90,
                column: "Codigo",
                value: "M-05");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 91,
                column: "Codigo",
                value: "M-06");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 92,
                column: "Codigo",
                value: "M-07");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 93,
                column: "Codigo",
                value: "M-08");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 94,
                column: "Codigo",
                value: "M-09");

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 95,
                column: "Codigo",
                value: "M-10");

            migrationBuilder.InsertData(
                table: "Cupos",
                columns: new[] { "Id", "Codigo", "Tipo" },
                values: new object[,]
                {
                    { 96, "M-11", 1 },
                    { 97, "M-12", 1 },
                    { 98, "M-13", 1 },
                    { 99, "M-14", 1 },
                    { 100, "M-15", 1 },
                    { 101, "M-16", 1 },
                    { 102, "M-17", 1 },
                    { 103, "M-18", 1 },
                    { 104, "M-19", 1 },
                    { 105, "M-20", 1 }
                });
        }
    }
}
