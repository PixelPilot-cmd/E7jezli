using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E7jezli.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Businesses",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Businesses",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserEmail",
                table: "Bookings",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_Category",
                table: "Businesses",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_Location",
                table: "Businesses",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_ServiceType",
                table: "Businesses",
                column: "ServiceType");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_Username",
                table: "Businesses",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BusinessId",
                table: "Bookings",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BusinessId_StartTime_EndTime",
                table: "Bookings",
                columns: new[] { "BusinessId", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BusinessId_Status",
                table: "Bookings",
                columns: new[] { "BusinessId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StartTime",
                table: "Bookings",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status",
                table: "Bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserEmail",
                table: "Bookings",
                column: "UserEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_Category",
                table: "Businesses");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_Location",
                table: "Businesses");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_ServiceType",
                table: "Businesses");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_Username",
                table: "Businesses");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BusinessId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BusinessId_StartTime_EndTime",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BusinessId_Status",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_StartTime",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_Status",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_UserEmail",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Businesses",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Businesses",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "UserEmail",
                table: "Bookings",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);
        }
    }
}
