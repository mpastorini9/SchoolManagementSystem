using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.DTOs;
using SchoolManagementSystem.Api.Models;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly SchoolContext _context;

        public ImportController(SchoolContext context) => _context = context;

        [HttpPost("preview")]
        public IActionResult Preview([FromBody] ImportPreviewRequest request)
        {
            var preview = BuildPreview(request);
            return Ok(preview);
        }

        [HttpPost("confirm")]
        public IActionResult Confirm([FromBody] ImportPreviewRequest request)
        {
            var preview = BuildPreview(request);
            if (!preview.CanConfirm) return BadRequest(preview);

            var rows = ParseRows(request.Content).Skip(1).ToList();
            switch (request.EntityType.Trim().ToLowerInvariant())
            {
                case "courses":
                    _context.Courses.AddRange(rows.Select(row => new Course { Name = row[0].Trim() }));
                    break;
                case "teachers":
                    _context.Teachers.AddRange(rows.Select(row => new Teacher
                    {
                        FirstName = row[0].Trim(), LastName = row[1].Trim(),
                        DocumentNumber = row[2].Trim(), IsActive = true
                    }));
                    break;
                case "students":
                    var courses = _context.Courses.ToList();
                    _context.Students.AddRange(rows.Select(row => new Student
                    {
                        FirstName = row[0].Trim(), LastName = row[1].Trim(), DocumentNumber = row[2].Trim(),
                        DateOfBirth = DateTime.ParseExact(row[3].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        CourseId = courses.Single(course => Same(course.Name, row[4])).Id
                    }));
                    break;
            }

            _context.SaveChanges();
            return Ok(new { importedRows = preview.ValidRows });
        }

        private ImportPreviewResponse BuildPreview(ImportPreviewRequest request)
        {
            var entityType = request.EntityType?.Trim().ToLowerInvariant() ?? string.Empty;
            var expectedHeaders = entityType switch
            {
                "courses" => new[] { "name" },
                "teachers" => new[] { "firstname", "lastname", "documentnumber" },
                "students" => new[] { "firstname", "lastname", "documentnumber", "dateofbirth", "coursename" },
                _ => Array.Empty<string>()
            };
            var response = new ImportPreviewResponse { EntityType = entityType };
            if (expectedHeaders.Length == 0)
            {
                response.Rows.Add(new ImportRowResult { RowNumber = 0, Message = "EntityType must be Courses, Teachers or Students." });
                return response;
            }

            var rows = ParseRows(request.Content);
            if (rows.Count == 0)
            {
                response.Rows.Add(new ImportRowResult { RowNumber = 0, Message = "The file is empty." });
                return response;
            }
            if (!HeadersMatch(rows[0], expectedHeaders))
            {
                response.Rows.Add(new ImportRowResult
                {
                    RowNumber = 1,
                    Message = $"Expected header: {string.Join(",", expectedHeaders)}.",
                    Values = rows[0]
                });
                return response;
            }

            response.TotalRows = rows.Count - 1;
            var knownCourses = _context.Courses.Select(course => course.Name).ToList();
            var knownDocuments = entityType == "teachers"
                ? _context.Teachers.Select(teacher => teacher.DocumentNumber).ToList()
                : _context.Students.Select(student => student.DocumentNumber).ToList();
            var fileKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var index = 1; index < rows.Count; index++)
            {
                var row = rows[index];
                var result = new ImportRowResult { RowNumber = index + 1, Values = row, IsValid = true };
                if (row.Count != expectedHeaders.Length || row.Any(string.IsNullOrWhiteSpace))
                    result.Message = "All required columns must have a value.";
                else if (entityType == "courses")
                {
                    var key = row[0].Trim();
                    if (!fileKeys.Add(key)) result.Message = "Duplicate course in the file.";
                    else if (knownCourses.Any(course => Same(course, key))) result.Message = "A course with this name already exists.";
                }
                else
                {
                    var documentNumber = row[2].Trim();
                    if (!documentNumber.All(char.IsDigit)) result.Message = "DocumentNumber must contain only numbers.";
                    else if (!fileKeys.Add(documentNumber)) result.Message = "Duplicate document number in the file.";
                    else if (knownDocuments.Any(document => Same(document, documentNumber))) result.Message = "A record with this document number already exists.";
                    else if (entityType == "students" && !DateTime.TryParseExact(row[3].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) result.Message = "DateOfBirth must use yyyy-MM-dd.";
                    else if (entityType == "students" && !knownCourses.Any(course => Same(course, row[4]))) result.Message = "CourseName does not match an existing course.";
                }

                result.IsValid = string.IsNullOrEmpty(result.Message);
                if (result.IsValid) response.ValidRows++;
                response.Rows.Add(result);
            }

            response.CanConfirm = response.TotalRows > 0 && response.ValidRows == response.TotalRows;
            return response;
        }

        private static bool HeadersMatch(List<string> actual, string[] expected) =>
            actual.Count == expected.Length && actual.Select(value => value.Trim().TrimStart('\uFEFF').ToLowerInvariant()).SequenceEqual(expected);

        private static bool Same(string left, string right) => string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);

        private static List<List<string>> ParseRows(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return new List<List<string>>();
            var delimiter = content.Contains(';') ? ';' : content.Contains('\t') ? '\t' : ',';
            return content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => ParseLine(line, delimiter)).ToList();
        }

        private static List<string> ParseLine(string line, char delimiter)
        {
            var values = new List<string>();
            var current = new System.Text.StringBuilder();
            var quoted = false;
            for (var index = 0; index < line.Length; index++)
            {
                if (line[index] == '"' && index + 1 < line.Length && line[index + 1] == '"') { current.Append('"'); index++; }
                else if (line[index] == '"') quoted = !quoted;
                else if (line[index] == delimiter && !quoted) { values.Add(current.ToString()); current.Clear(); }
                else current.Append(line[index]);
            }
            values.Add(current.ToString());
            return values;
        }
    }
}
