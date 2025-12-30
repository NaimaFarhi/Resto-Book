using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestoBook_MiniProjet.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialRequestToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpecialRequest",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecialRequest",
                table: "Reservations");
        }
    }
}
