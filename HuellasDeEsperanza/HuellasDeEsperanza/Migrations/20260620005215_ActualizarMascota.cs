using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuellasDeEsperanza.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarMascota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Adoptado",
                table: "Mascotas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Disponibilidad",
                table: "Mascotas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Transitado",
                table: "Mascotas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adoptado",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "Disponibilidad",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "Transitado",
                table: "Mascotas");
        }
    }
}
