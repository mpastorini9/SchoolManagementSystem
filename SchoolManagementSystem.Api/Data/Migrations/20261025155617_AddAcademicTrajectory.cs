using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagementSystem.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicTrajectory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Subjects_Id_SchoolId",
                table: "Subjects",
                columns: new[] { "Id", "SchoolId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Courses_Id_SchoolId",
                table: "Courses",
                columns: new[] { "Id", "SchoolId" });

            migrationBuilder.CreateTable(
                name: "AcademicEvaluations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicEvaluations", x => x.Id);
                    table.UniqueConstraint("AK_AcademicEvaluations_Id_SchoolId", x => new { x.Id, x.SchoolId });
                    table.ForeignKey(
                        name: "FK_AcademicEvaluations_Courses_CourseId_SchoolId",
                        columns: x => new { x.CourseId, x.SchoolId },
                        principalTable: "Courses",
                        principalColumns: new[] { "Id", "SchoolId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AcademicEvaluations_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AcademicEvaluations_Subjects_SubjectId_SchoolId",
                        columns: x => new { x.SubjectId, x.SchoolId },
                        principalTable: "Subjects",
                        principalColumns: new[] { "Id", "SchoolId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AcademicEvaluations_Teachers_TeacherId_SchoolId",
                        columns: x => new { x.TeacherId, x.SchoolId },
                        principalTable: "Teachers",
                        principalColumns: new[] { "Id", "SchoolId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentRecordEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVisibleToFamily = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentRecordEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentRecordEntries_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentRecordEntries_Students_StudentId_SchoolId",
                        columns: x => new { x.StudentId, x.SchoolId },
                        principalTable: "Students",
                        principalColumns: new[] { "Id", "SchoolId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentGrades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    EvaluationId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentGrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentGrades_AcademicEvaluations_EvaluationId_SchoolId",
                        columns: x => new { x.EvaluationId, x.SchoolId },
                        principalTable: "AcademicEvaluations",
                        principalColumns: new[] { "Id", "SchoolId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentGrades_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentGrades_Students_StudentId_SchoolId",
                        columns: x => new { x.StudentId, x.SchoolId },
                        principalTable: "Students",
                        principalColumns: new[] { "Id", "SchoolId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicEvaluations_CourseId_SchoolId",
                table: "AcademicEvaluations",
                columns: new[] { "CourseId", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicEvaluations_SchoolId",
                table: "AcademicEvaluations",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicEvaluations_SubjectId_SchoolId",
                table: "AcademicEvaluations",
                columns: new[] { "SubjectId", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicEvaluations_TeacherId_SchoolId",
                table: "AcademicEvaluations",
                columns: new[] { "TeacherId", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentGrades_EvaluationId_SchoolId",
                table: "StudentGrades",
                columns: new[] { "EvaluationId", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentGrades_EvaluationId_StudentId_SchoolId",
                table: "StudentGrades",
                columns: new[] { "EvaluationId", "StudentId", "SchoolId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentGrades_SchoolId",
                table: "StudentGrades",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGrades_StudentId_SchoolId",
                table: "StudentGrades",
                columns: new[] { "StudentId", "SchoolId" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentRecordEntries_SchoolId",
                table: "StudentRecordEntries",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRecordEntries_StudentId_SchoolId_RecordedAt",
                table: "StudentRecordEntries",
                columns: new[] { "StudentId", "SchoolId", "RecordedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentGrades");

            migrationBuilder.DropTable(
                name: "StudentRecordEntries");

            migrationBuilder.DropTable(
                name: "AcademicEvaluations");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Subjects_Id_SchoolId",
                table: "Subjects");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Courses_Id_SchoolId",
                table: "Courses");
        }
    }
}
