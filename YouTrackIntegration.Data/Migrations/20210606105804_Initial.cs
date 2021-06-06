using Microsoft.EntityFrameworkCore.Migrations;

namespace YouTrackIntegration.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Associations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    workspaceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    domain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    permToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    defaultIssueId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Associations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClockifyYouTrackAssociationId = table.Column<int>(type: "int", nullable: false),
                    clockifyUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    youTrackUserLogin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    defaultIssueId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Associations",
                columns: new[] { "Id", "defaultIssueId", "domain", "permToken", "workspaceId" },
                values: new object[] { 1, "CAYTI-2", "https://coursework.myjetbrains.com/youtrack", "perm:cm9vdA==.NDktMA==.dGM8QEEi9ToWNX7Xta2wSDFdN2xCbE", "608ab876b2a2b737f32693d9" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ClockifyYouTrackAssociationId", "clockifyUserId", "defaultIssueId", "youTrackUserLogin" },
                values: new object[] { 1, 1, "608ab876b2a2b737f32693d8", null, "pesoshin" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Associations");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
