using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Parqueadero.Core.Migrations
{
    /// <inheritdoc />
    public partial class Plano105Cupos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-41", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-42", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-43", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-44", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-45", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-46", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-47", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-48", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-49", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-50", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-51", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 52,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-52", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-53", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-54", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-55", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 56,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-56", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-57", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 58,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-58", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 59,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-59", 0 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 60,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "A-60", 0 });

            migrationBuilder.InsertData(
                table: "Cupos",
                columns: new[] { "Id", "Codigo", "Tipo" },
                values: new object[,]
                {
                    { 61, "A-61", 0 },
                    { 62, "A-62", 0 },
                    { 63, "A-63", 0 },
                    { 64, "A-64", 0 },
                    { 65, "A-65", 0 },
                    { 66, "A-66", 0 },
                    { 67, "A-67", 0 },
                    { 68, "A-68", 0 },
                    { 69, "A-69", 0 },
                    { 70, "A-70", 0 },
                    { 71, "A-71", 0 },
                    { 72, "A-72", 0 },
                    { 73, "A-73", 0 },
                    { 74, "A-74", 0 },
                    { 75, "A-75", 0 },
                    { 76, "A-76", 0 },
                    { 77, "A-77", 0 },
                    { 78, "A-78", 0 },
                    { 79, "A-79", 0 },
                    { 80, "A-80", 0 },
                    { 81, "A-81", 0 },
                    { 82, "A-82", 0 },
                    { 83, "A-83", 0 },
                    { 84, "A-84", 0 },
                    { 85, "A-85", 0 },
                    { 86, "M-01", 1 },
                    { 87, "M-02", 1 },
                    { 88, "M-03", 1 },
                    { 89, "M-04", 1 },
                    { 90, "M-05", 1 },
                    { 91, "M-06", 1 },
                    { 92, "M-07", 1 },
                    { 93, "M-08", 1 },
                    { 94, "M-09", 1 },
                    { 95, "M-10", 1 },
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 88);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 89);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 90);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 95);

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
                keyValue: 41,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-01", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-02", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-03", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-04", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-05", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-06", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-07", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-08", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-09", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-10", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-11", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 52,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-12", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-13", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-14", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-15", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 56,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-16", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-17", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 58,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-18", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 59,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-19", 1 });

            migrationBuilder.UpdateData(
                table: "Cupos",
                keyColumn: "Id",
                keyValue: 60,
                columns: new[] { "Codigo", "Tipo" },
                values: new object[] { "M-20", 1 });
        }
    }
}
