using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagementSystem.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyStudentLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_AspNetUsers_Id_SchoolId",
                table: "AspNetUsers",
                columns: new[] { "Id", "SchoolId" });

            migrationBuilder.CreateTable(
                name: "FamilyStudentLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    FamilyUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    LinkedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LinkedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyStudentLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyStudentLinks_AspNetUsers_FamilyUserId_SchoolId",
                        columns: x => new { x.FamilyUserId, x.SchoolId },
                        principalTable: "AspNetUsers",
                        principalColumns: new[] { "Id", "SchoolId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyStudentLinks_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyStudentLinks_Students_StudentId_SchoolId",
                        columns: x => new { x.StudentId, x.SchoolId },
                        principalTable: "Students",
                        principalColumns: new[] { "Id", "SchoolId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyStudentLinks_FamilyUserId_SchoolId",
                table: "FamilyStudentLinks",
                columns: new[] { "FamilyUserId", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyStudentLinks_FamilyUserId_StudentId_SchoolId",
                table: "FamilyStudentLinks",
                columns: new[] { "FamilyUserId", "StudentId", "SchoolId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyStudentLinks_SchoolId",
                table: "FamilyStudentLinks",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyStudentLinks_StudentId_SchoolId",
                table: "FamilyStudentLinks",
                columns: new[] { "StudentId", "SchoolId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FamilyStudentLinks");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AspNetUsers_Id_SchoolId",
                table: "AspNetUsers");
        }
    }
}
