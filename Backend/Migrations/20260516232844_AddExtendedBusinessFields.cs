using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E7jezli.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedBusinessFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraServices",
                table: "Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryImages",
                table: "Businesses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "ExtraServices",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "SecondaryImages",
                table: "Businesses");
        }
    }
}
