using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetSuiteIntegration.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupCountryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LookupCountry",
                columns: table => new
                {
                    UNITeCountryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NetSuiteCountryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupCountry", x => x.UNITeCountryCode);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LookupCountry");
        }
    }
}
