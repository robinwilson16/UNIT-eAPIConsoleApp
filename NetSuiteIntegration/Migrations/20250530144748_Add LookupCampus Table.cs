using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetSuiteIntegration.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupCampusTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LookupCampus",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RefName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UNITeCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupCampus", x => x.ID);
                });

            //migrationBuilder.CreateTable(
            //    name: "Settings",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Enviroment = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
            //        Enabled = table.Column<bool>(type: "bit", nullable: true),
            //        NetSuiteAccountID = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //        NetSuiteURL = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //        NetSuiteConsumerKey = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //        NetSuiteTokenID = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //        NetSuiteConsumerSecret = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //        NetSuiteTokenSecret = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //        UniteBaseURL = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //        UniteTokenURL = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
            //        UniteAPIKey = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Settings", x => x.Id);
            //    });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LookupCampus");

            //migrationBuilder.DropTable(
            //    name: "Settings");
        }
    }
}
