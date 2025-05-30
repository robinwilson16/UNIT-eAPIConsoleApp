using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetSuiteIntegration.Migrations
{
    /// <inheritdoc />
    public partial class AlterLookupCountryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetSuiteCountryCode",
                table: "LookupCountry");

            migrationBuilder.AddColumn<string>(
                name: "NetSuiteCountryName",
                table: "LookupCountry",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NetSuiteCountryName",
                table: "LookupCountry");

            migrationBuilder.AddColumn<string>(
                name: "NetSuiteCountryCode",
                table: "LookupCountry",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
