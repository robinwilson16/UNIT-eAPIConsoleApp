using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetSuiteIntegration.Migrations
{
    /// <inheritdoc />
    public partial class AmendLookupCampusTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UNITeCode",
                table: "LookupCampus",
                newName: "NetSuiteSubsiduaryID");

            migrationBuilder.RenameColumn(
                name: "RefName",
                table: "LookupCampus",
                newName: "NetSuiteLocationName");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "LookupCampus",
                newName: "UNITeCampusCode");

            migrationBuilder.AddColumn<string>(
                name: "NetSuiteFacultyID",
                table: "LookupCampus",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NetSuiteLocationID",
                table: "LookupCampus",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetSuiteFacultyID",
                table: "LookupCampus");

            migrationBuilder.DropColumn(
                name: "NetSuiteLocationID",
                table: "LookupCampus");

            migrationBuilder.RenameColumn(
                name: "NetSuiteSubsiduaryID",
                table: "LookupCampus",
                newName: "UNITeCode");

            migrationBuilder.RenameColumn(
                name: "NetSuiteLocationName",
                table: "LookupCampus",
                newName: "RefName");

            migrationBuilder.RenameColumn(
                name: "UNITeCampusCode",
                table: "LookupCampus",
                newName: "ID");
        }
    }
}
