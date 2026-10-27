using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.Models;
using SchoolManagementSystem.Api.Services;
using Xunit;

namespace SchoolManagementSystem.Api.SecurityTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SecurityTestCollection : ICollectionFixture<SecurityTestHost>
{
    public const string Name = "school security";
}

[Collection(SecurityTestCollection.Name)]
public sealed class SchoolManagementSecurityTests : IAsyncLifetime
{
    private readonly SecurityTestHost _host;

    public SchoolManagementSecurityTests(SecurityTestHost host) => _host = host;

    public async Task InitializeAsync() => await _host.InitializeAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Protected_submission_and_material_routes_reject_anonymous_requests()
    {
        using var client = _host.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/student/submissions")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/student/submissions/6001/revisions")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/teacher/activities/3001/submissions")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PutAsJsonAsync("/api/teacher/submissions/6001/feedback", new { Observation = "x" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/Activity/3001/materials")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/Activity/3001/materials/8001/file")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/family/students")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/family/students/1001/attendance")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/family/students/1001/activities")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/family/students/1001/schedule")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/family/students/1001/grades")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/family/students/1001/records")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/family/students/1001/activities/3001/materials")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.GetAsync("/api/family/students/1001/activities/3001/materials/8001/file")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/admin/family-links")).StatusCode);
    }

    [Fact]
    public async Task Student_can_access_only_own_submissions_and_revision_files()
    {
        var student = await _host.AuthenticateAsync(_host.StudentOne);
        using var client = student.Client;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", student.AccessToken);

        using var list = JsonDocument.Parse(await client.GetStringAsync("/api/student/submissions"));
        Assert.Equal(new[] { 6001 }, list.RootElement.EnumerateArray().Select(item => item.GetProperty("id").GetInt32()));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/student/submissions/6001/revisions")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/student/submissions/6002/revisions")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/student/submissions/9001/revisions")).StatusCode);

        var ownFile = await client.GetAsync("/api/student/submissions/6001/revisions/7001/file");
        Assert.Equal(HttpStatusCode.OK, ownFile.StatusCode);
        Assert.Equal("student-one-file", await ownFile.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync("/api/student/submissions/6002/revisions/7002/file")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync("/api/student/submissions/9001/revisions/9002/file")).StatusCode);

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("submitted from student one"), "value");
        form.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("new-student-file")), "file", "answer.txt");
        Assert.Equal(HttpStatusCode.OK,
            (await client.PostAsync("/api/student/activities/3001/submissions", form)).StatusCode);

        using var crossSchoolForm = new MultipartFormDataContent();
        crossSchoolForm.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("must not be saved")), "file", "foreign.txt");
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PostAsync("/api/student/activities/4001/submissions", crossSchoolForm)).StatusCode);
    }

    [Fact]
    public async Task Teacher_can_review_only_activities_assigned_to_their_account_and_school()
    {
        var teacher = await _host.AuthenticateAsync(_host.TeacherOne);
        using var client = teacher.Client;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", teacher.AccessToken);

        Assert.Equal(HttpStatusCode.OK,
            (await client.GetAsync("/api/teacher/activities/3001/submissions")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync("/api/teacher/activities/3002/submissions")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync("/api/teacher/activities/4001/submissions")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await client.GetAsync("/api/teacher/submissions/6002/revisions")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync("/api/teacher/submissions/9001/revisions")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync("/api/teacher/submissions/9001/revisions/9002/file")).StatusCode);
    }

    [Fact]
    public async Task Administrator_queries_and_writes_are_limited_to_the_claimed_school()
    {
        var schoolOneAdmin = await _host.AuthenticateAsync(_host.AdminOne);
        using var client = schoolOneAdmin.Client;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", schoolOneAdmin.AccessToken);

        using var courses = JsonDocument.Parse(await client.GetStringAsync("/api/Course"));
        var ids = courses.RootElement.EnumerateArray().Select(item => item.GetProperty("id").GetInt32()).ToArray();
        Assert.Contains(1001, ids);
        Assert.DoesNotContain(2001, ids);

        var attemptedCrossSchoolCourse = await client.PostAsJsonAsync("/api/Course", new Course
        {
            Name = "Must belong to school one", SchoolId = 2
        });
        Assert.Equal(HttpStatusCode.Created, attemptedCrossSchoolCourse.StatusCode);
        using var created = JsonDocument.Parse(await attemptedCrossSchoolCourse.Content.ReadAsStringAsync());
        Assert.Equal(1, created.RootElement.GetProperty("schoolId").GetInt32());

        var provisionCrossSchoolStudent = await client.PostAsJsonAsync("/api/admin/accounts", new
        {
            Email = _host.NewEmail(), Password = _host.NewPassword(), Role = "Student", StudentId = 2002, SchoolId = 1
        });
        Assert.Equal(HttpStatusCode.BadRequest, provisionCrossSchoolStudent.StatusCode);

        var schoolTwoAdmin = await _host.AuthenticateAsync(_host.AdminTwo);
        using var otherClient = schoolTwoAdmin.Client;
        otherClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", schoolTwoAdmin.AccessToken);
        using var otherCourses = JsonDocument.Parse(await otherClient.GetStringAsync("/api/Course"));
        Assert.Contains(2001, otherCourses.RootElement.EnumerateArray().Select(item => item.GetProperty("id").GetInt32()));
        Assert.DoesNotContain(1001, otherCourses.RootElement.EnumerateArray().Select(item => item.GetProperty("id").GetInt32()));
    }

    [Fact]
    public async Task Activity_material_uploads_and_downloads_are_role_and_school_scoped()
    {
        var teacher = await _host.AuthenticateAsync(_host.TeacherOne);
        using var client = teacher.Client;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", teacher.AccessToken);

        using var upload = new MultipartFormDataContent();
        upload.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("course material")), "file", "material.txt");
        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsync("/api/Activity/3003/materials/file", upload)).StatusCode);

        using var crossSchoolUpload = new MultipartFormDataContent();
        crossSchoolUpload.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("foreign material")), "file", "foreign.txt");
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PostAsync("/api/Activity/4001/materials/file", crossSchoolUpload)).StatusCode);

        var student = await _host.AuthenticateAsync(_host.StudentOne);
        using var studentClient = student.Client;
        studentClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", student.AccessToken);
        Assert.Equal(HttpStatusCode.OK,
            (await studentClient.GetAsync("/api/Activity/3001/materials/8001/file")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await studentClient.GetAsync("/api/Activity/4001/materials/8002/file")).StatusCode);
        using var anonymous = _host.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await anonymous.GetAsync("/api/Activity/3001/materials/8001/file")).StatusCode);
    }

    [Fact]
    public async Task Family_accounts_can_read_only_linked_students_attendance_activities_and_materials()
    {
        var family = await _host.AuthenticateAsync(_host.FamilyOne);
        using var client = family.Client;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", family.AccessToken);

        using var students = JsonDocument.Parse(await client.GetStringAsync("/api/family/students"));
        var studentIds = students.RootElement.EnumerateArray().Select(item => item.GetProperty("id").GetInt32()).ToArray();
        Assert.Equal(new[] { 1001, 1002 }, studentIds);

        using var attendance = JsonDocument.Parse(await client.GetStringAsync("/api/family/students/1001/attendance"));
        Assert.Equal("Absent", attendance.RootElement[0].GetProperty("status").GetString());
        var unlinkedAttendance = await client.GetAsync("/api/family/students/1003/attendance");
        await AssertNotFoundWithoutPrivateDataAsync(unlinkedAttendance, "Student Three", _host.FamilyOtherSchool.Email);
        var crossSchoolAttendance = await client.GetAsync("/api/family/students/2001/attendance");
        await AssertNotFoundWithoutPrivateDataAsync(crossSchoolAttendance,
            "Other School Student", "School Two Course", _host.FamilyOtherSchool.Email);

        using var activities = JsonDocument.Parse(await client.GetStringAsync("/api/family/students/1001/activities"));
        var activityIds = activities.RootElement.EnumerateArray().Select(item => item.GetProperty("id").GetInt32()).ToArray();
        Assert.Equal(new[] { 3001 }, activityIds);
        var familySubmission = activities.RootElement[0].GetProperty("submission");
        Assert.Equal(6001, familySubmission.GetProperty("id").GetInt32());
        Assert.NotEmpty(familySubmission.GetProperty("revisions").EnumerateArray());
        var submittedFile = await client.GetAsync($"/api/family/students/1001/activities/3001/submissions/6001/revisions/7001/file");
        Assert.Equal(HttpStatusCode.OK, submittedFile.StatusCode);
        Assert.Equal("student-one-file", await submittedFile.Content.ReadAsStringAsync());
        var unauthorizedSubmittedFile = await client.GetAsync("/api/family/students/1003/activities/3001/submissions/6001/revisions/7001/file");
        await AssertNotFoundWithoutPrivateDataAsync(unauthorizedSubmittedFile, "Student Three", "test answer", _host.FamilyOtherSchool.Email);
        Assert.DoesNotContain("teacherObservation", activities.RootElement.GetRawText(), StringComparison.OrdinalIgnoreCase);
        var unassignedActivityMaterials = await client.GetAsync("/api/family/students/1001/activities/3002/materials");
        await AssertNotFoundWithoutPrivateDataAsync(unassignedActivityMaterials,
            "Student Two", "Other School Student", _host.FamilyOtherSchool.Email);

        using var materials = JsonDocument.Parse(await client.GetStringAsync("/api/family/students/1001/activities/3001/materials"));
        Assert.Equal(8001, materials.RootElement[0].GetProperty("id").GetInt32());
        var file = await client.GetAsync("/api/family/students/1001/activities/3001/materials/8001/file");
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
        Assert.Equal("school-one-material", await file.Content.ReadAsStringAsync());
        var unlinkedFile = await client.GetAsync("/api/family/students/1003/activities/3001/materials/8001/file");
        await AssertNotFoundWithoutPrivateDataAsync(unlinkedFile,
            "Student Three", "Other School Student", _host.FamilyOtherSchool.Email, "other school material");

        var secondAdult = await _host.AuthenticateAsync(_host.FamilyTwo);
        using var secondAdultClient = secondAdult.Client;
        secondAdultClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secondAdult.AccessToken);
        Assert.Equal(HttpStatusCode.OK, (await secondAdultClient.GetAsync("/api/family/students/1001/attendance")).StatusCode);
    }

    [Fact]
    public async Task Family_academic_endpoints_are_link_scoped_and_read_only()
    {
        var family = await _host.AuthenticateAsync(_host.FamilyOne);
        using var client = family.Client;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", family.AccessToken);

        foreach (var endpoint in new[] { "schedule", "grades", "records" })
        {
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/family/students/1001/{endpoint}")).StatusCode);
            var unlinked = await client.GetAsync($"/api/family/students/1003/{endpoint}");
            await AssertNotFoundWithoutPrivateDataAsync(unlinked, "Student Three", "Other School Student", _host.FamilyOtherSchool.Email);
            var crossSchool = await client.GetAsync($"/api/family/students/2001/{endpoint}");
            await AssertNotFoundWithoutPrivateDataAsync(crossSchool, "Other School Student", "School Two Course", _host.FamilyOtherSchool.Email);
        }

        Assert.Equal(HttpStatusCode.Forbidden,
            (await client.PostAsJsonAsync("/api/teacher/evaluations", new { CourseId = 1001, SubjectId = 1001, Title = "No", EvaluatedAt = DateTime.UtcNow })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await client.PutAsJsonAsync("/api/teacher/grades/1", new { Value = "No" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await client.PostAsJsonAsync("/api/admin/students/1001/records", new { Category = "Observation", Title = "No", Description = "No", RecordedAt = DateTime.UtcNow })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await client.PutAsJsonAsync("/api/teacher/submissions/6001/feedback", new { Observation = "No" })).StatusCode);
    }

    [Fact]
    public async Task Teachers_can_manage_only_assigned_grades_and_families_see_only_published_results()
    {
        var teacher = await _host.AuthenticateAsync(_host.TeacherOne);
        using var teacherClient = teacher.Client;
        teacherClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", teacher.AccessToken);

        var evaluationResponse = await teacherClient.PostAsJsonAsync("/api/teacher/evaluations", new
        {
            CourseId = 1001, SubjectId = 1001, Title = "Evaluación de prueba", EvaluatedAt = DateTime.UtcNow
        });
        Assert.Equal(HttpStatusCode.Created, evaluationResponse.StatusCode);
        using var evaluationJson = JsonDocument.Parse(await evaluationResponse.Content.ReadAsStringAsync());
        var evaluationId = evaluationJson.RootElement.GetProperty("id").GetInt32();
        var gradeResponse = await teacherClient.PostAsJsonAsync($"/api/teacher/evaluations/{evaluationId}/grades", new { StudentId = 1001, Value = "8" });
        Assert.Equal(HttpStatusCode.Created, gradeResponse.StatusCode);
        using var gradeJson = JsonDocument.Parse(await gradeResponse.Content.ReadAsStringAsync());
        var gradeId = gradeJson.RootElement.GetProperty("id").GetInt32();

        var family = await _host.AuthenticateAsync(_host.FamilyOne);
        using var familyClient = family.Client;
        familyClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", family.AccessToken);
        using var beforePublish = JsonDocument.Parse(await familyClient.GetStringAsync("/api/family/students/1001/grades"));
        Assert.Empty(beforePublish.RootElement.EnumerateArray());

        Assert.Equal(HttpStatusCode.NoContent, (await teacherClient.PostAsync($"/api/teacher/grades/{gradeId}/publish", null)).StatusCode);
        using var afterPublish = JsonDocument.Parse(await familyClient.GetStringAsync("/api/family/students/1001/grades"));
        Assert.Equal("8", afterPublish.RootElement[0].GetProperty("value").GetString());
        Assert.DoesNotContain("createdByUserId", afterPublish.RootElement.GetRawText(), StringComparison.OrdinalIgnoreCase);

        Assert.Equal(HttpStatusCode.NotFound,
            (await teacherClient.PostAsJsonAsync($"/api/teacher/evaluations/{evaluationId}/grades", new { StudentId = 2001, Value = "7" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await teacherClient.PostAsJsonAsync("/api/teacher/evaluations", new
            {
                CourseId = 2001, SubjectId = 2001, Title = "Curso ajeno", EvaluatedAt = DateTime.UtcNow
            })).StatusCode);

        var administrator = await _host.AuthenticateAsync(_host.AdminOne);
        using var adminClient = administrator.Client;
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administrator.AccessToken);
        var otherTeacherEvaluation = await adminClient.PostAsJsonAsync("/api/teacher/evaluations", new
        {
            CourseId = 1001, SubjectId = 1001, TeacherId = 1002, Title = "Otra asignación", EvaluatedAt = DateTime.UtcNow
        });
        Assert.Equal(HttpStatusCode.Created, otherTeacherEvaluation.StatusCode);
        using var otherEvaluationJson = JsonDocument.Parse(await otherTeacherEvaluation.Content.ReadAsStringAsync());
        var otherEvaluationId = otherEvaluationJson.RootElement.GetProperty("id").GetInt32();
        var otherGradeResponse = await adminClient.PostAsJsonAsync($"/api/teacher/evaluations/{otherEvaluationId}/grades", new { StudentId = 1001, Value = "7" });
        Assert.Equal(HttpStatusCode.Created, otherGradeResponse.StatusCode);
        using var otherGradeJson = JsonDocument.Parse(await otherGradeResponse.Content.ReadAsStringAsync());
        var otherGradeId = otherGradeJson.RootElement.GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.Forbidden,
            (await teacherClient.PutAsJsonAsync($"/api/teacher/grades/{otherGradeId}", new { Value = "9" })).StatusCode);
    }

    [Fact]
    public async Task Submission_feedback_is_written_by_the_responsible_teacher_and_visible_to_linked_family()
    {
        var teacher = await _host.AuthenticateAsync(_host.TeacherOne);
        using var teacherClient = teacher.Client;
        teacherClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", teacher.AccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await teacherClient.PutAsJsonAsync(
            "/api/teacher/submissions/6001/feedback", new { Observation = "Buen trabajo, revisá el segundo ejercicio." })).StatusCode);

        var family = await _host.AuthenticateAsync(_host.FamilyOne);
        using var familyClient = family.Client;
        familyClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", family.AccessToken);
        using var activities = JsonDocument.Parse(await familyClient.GetStringAsync("/api/family/students/1001/activities"));
        var submission = activities.RootElement[0].GetProperty("submission");
        Assert.Equal("Buen trabajo, revisá el segundo ejercicio.", submission.GetProperty("teacherFeedback").GetString());
        Assert.NotEqual(JsonValueKind.Null, submission.GetProperty("reviewedAt").ValueKind);

        var otherTeacher = await _host.AuthenticateAsync(_host.TeacherTwo);
        using var otherTeacherClient = otherTeacher.Client;
        otherTeacherClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherTeacher.AccessToken);
        Assert.Equal(HttpStatusCode.NotFound, (await otherTeacherClient.PutAsJsonAsync(
            "/api/teacher/submissions/6001/feedback", new { Observation = "No autorizado" })).StatusCode);
        Assert.DoesNotContain("No autorizado", await familyClient.GetStringAsync("/api/family/students/1001/activities"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Family_records_require_explicit_visibility_and_never_include_confidential_notes()
    {
        var administrator = await _host.AuthenticateAsync(_host.AdminOne);
        using var adminClient = administrator.Client;
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administrator.AccessToken);
        var visible = await adminClient.PostAsJsonAsync("/api/admin/students/1001/records", new
        {
            Category = "Recognition", Title = "Reconocimiento", Description = "Detalle visible", RecordedAt = DateTime.UtcNow, IsVisibleToFamily = true
        });
        Assert.Equal(HttpStatusCode.Created, visible.StatusCode);
        var hidden = await adminClient.PostAsJsonAsync("/api/admin/students/1001/records", new
        {
            Category = "Observation", Title = "Nota interna", Description = "Privado", RecordedAt = DateTime.UtcNow, IsVisibleToFamily = false
        });
        Assert.Equal(HttpStatusCode.Created, hidden.StatusCode);
        var confidential = await adminClient.PostAsJsonAsync("/api/admin/students/1001/records", new
        {
            Category = "ConfidentialNote", Title = "Confidencial", Description = "Secreto", RecordedAt = DateTime.UtcNow, IsVisibleToFamily = true
        });
        Assert.Equal(HttpStatusCode.BadRequest, confidential.StatusCode);

        var family = await _host.AuthenticateAsync(_host.FamilyOne);
        using var familyClient = family.Client;
        familyClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", family.AccessToken);
        using var records = JsonDocument.Parse(await familyClient.GetStringAsync("/api/family/students/1001/records"));
        Assert.Single(records.RootElement.EnumerateArray());
        Assert.Contains("Detalle visible", records.RootElement.GetRawText());
        await AssertResponseDoesNotContainAsync(records, "Privado", "Secreto", "Confidencial", "Nota interna");
    }

    [Fact]
    public async Task Authenticated_non_family_roles_cannot_access_family_portal_routes()
    {
        var student = await _host.AuthenticateAsync(_host.StudentOne);
        using var client = student.Client;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", student.AccessToken);

        var routes = new[]
        {
            "/api/family/students",
            "/api/family/students/1001/attendance",
            "/api/family/students/1001/activities",
            "/api/family/students/1001/schedule",
            "/api/family/students/1001/grades",
            "/api/family/students/1001/records",
            "/api/family/students/1001/activities/3001/materials",
            "/api/family/students/1001/activities/3001/materials/8001/file"
        };

        foreach (var route in routes)
            Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync(route)).StatusCode);
    }

    [Fact]
    public async Task Family_portal_rejects_unlinked_and_cross_school_student_ids_for_all_student_routes()
    {
        var family = await _host.AuthenticateAsync(_host.FamilyOne);
        using var client = family.Client;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", family.AccessToken);

        var privateValues = new[]
        {
            "Student Three", "Other School Student", "School Two Course",
            _host.FamilyOtherSchool.Email, "other school material"
        };
        var protectedRoutes = new[]
        {
            "/api/family/students/1003/activities",
            "/api/family/students/2001/activities",
            "/api/family/students/1003/activities/3001/materials",
            "/api/family/students/2001/activities/4001/materials",
            "/api/family/students/2001/activities/4001/materials/8002/file"
        };

        foreach (var route in protectedRoutes)
        {
            var response = await client.GetAsync(route);
            await AssertNotFoundWithoutPrivateDataAsync(response, privateValues);
        }
    }

    [Fact]
    public async Task Family_link_management_rejects_non_administrators_and_is_school_scoped()
    {
        foreach (var nonAdministrator in new[] { _host.StudentOne, _host.FamilyOne })
        {
            var authenticated = await _host.AuthenticateAsync(nonAdministrator);
            using var client = authenticated.Client;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticated.AccessToken);

            Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/admin/family-links")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/admin/family-links", new
            {
                FamilyUserId = "unauthorized-user", StudentId = 1001
            })).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden,
                (await client.DeleteAsync("/api/admin/family-links/unauthorized-user/1001")).StatusCode);
        }

        var schoolOneAdmin = await _host.AuthenticateAsync(_host.AdminOne);
        using var schoolOneClient = schoolOneAdmin.Client;
        schoolOneClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", schoolOneAdmin.AccessToken);
        using var schoolOneLinks = JsonDocument.Parse(await schoolOneClient.GetStringAsync("/api/admin/family-links"));
        var familyOneLink = schoolOneLinks.RootElement.EnumerateArray().Single(item =>
            item.GetProperty("email").GetString() == _host.FamilyOne.Email && item.GetProperty("studentId").GetInt32() == 1001);
        var familyOneId = familyOneLink.GetProperty("familyUserId").GetString()!;

        var schoolTwoAdmin = await _host.AuthenticateAsync(_host.AdminTwo);
        using var schoolTwoClient = schoolTwoAdmin.Client;
        schoolTwoClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", schoolTwoAdmin.AccessToken);

        using var schoolTwoLinks = JsonDocument.Parse(await schoolTwoClient.GetStringAsync("/api/admin/family-links"));
        Assert.Contains(schoolTwoLinks.RootElement.EnumerateArray(), item =>
            item.GetProperty("email").GetString() == _host.FamilyOtherSchool.Email && item.GetProperty("studentId").GetInt32() == 2001);
        Assert.DoesNotContain(schoolTwoLinks.RootElement.EnumerateArray(), item =>
            item.GetProperty("email").GetString() == _host.FamilyOne.Email || item.GetProperty("studentId").GetInt32() == 1001);
        await AssertResponseDoesNotContainAsync(schoolTwoLinks, _host.FamilyOne.Email, "Student One", "School One Course");

        using var filteredOtherSchoolLinks = JsonDocument.Parse(await schoolTwoClient.GetStringAsync(
            $"/api/admin/family-links?familyUserId={Uri.EscapeDataString(familyOneId)}"));
        Assert.Empty(filteredOtherSchoolLinks.RootElement.EnumerateArray());
        await AssertResponseDoesNotContainAsync(filteredOtherSchoolLinks, _host.FamilyOne.Email, "Student One", "School One Course");
        Assert.Equal(HttpStatusCode.NotFound,
            (await schoolTwoClient.DeleteAsync($"/api/admin/family-links/{Uri.EscapeDataString(familyOneId)}/1001")).StatusCode);

        var family = await _host.AuthenticateAsync(_host.FamilyOne);
        using var familyClient = family.Client;
        familyClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", family.AccessToken);
        Assert.Equal(HttpStatusCode.OK, (await familyClient.GetAsync("/api/family/students/1001/attendance")).StatusCode);
    }

    private static async Task AssertNotFoundWithoutPrivateDataAsync(HttpResponseMessage response, params string[] privateValues)
    {
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertResponseDoesNotContainAsync(response, privateValues);
    }

    private static async Task AssertResponseDoesNotContainAsync(HttpResponseMessage response, params string[] privateValues)
    {
        await AssertResponseDoesNotContainAsync(await response.Content.ReadAsStringAsync(), privateValues);
    }

    private static async Task AssertResponseDoesNotContainAsync(JsonDocument response, params string[] privateValues)
    {
        await AssertResponseDoesNotContainAsync(response.RootElement.GetRawText(), privateValues);
    }

    private static Task AssertResponseDoesNotContainAsync(string responseBody, params string[] privateValues)
    {
        foreach (var privateValue in privateValues.Where(value => !string.IsNullOrWhiteSpace(value)))
            Assert.DoesNotContain(privateValue, responseBody, StringComparison.OrdinalIgnoreCase);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task School_administrator_manages_family_links_and_family_account_provisioning_is_school_scoped()
    {
        var admin = await _host.AuthenticateAsync(_host.AdminOne);
        using var client = admin.Client;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.AccessToken);

        var email = _host.NewEmail();
        var password = _host.NewPassword();
        var created = await client.PostAsJsonAsync("/api/admin/accounts", new
        {
            Email = email, Password = password, Role = "Family", StudentIds = new[] { 1001 }
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdJson = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var familyUserId = createdJson.RootElement.GetProperty("id").GetString()!;

        var crossSchoolProvision = await client.PostAsJsonAsync("/api/admin/accounts", new
        {
            Email = _host.NewEmail(), Password = _host.NewPassword(), Role = "Family", StudentIds = new[] { 1001, 2001 }
        });
        Assert.Equal(HttpStatusCode.BadRequest, crossSchoolProvision.StatusCode);

        var added = await client.PostAsJsonAsync("/api/admin/family-links", new
        {
            FamilyUserId = familyUserId, StudentId = 1002
        });
        Assert.Equal(HttpStatusCode.Created, added.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/admin/family-links", new
        {
            FamilyUserId = familyUserId, StudentId = 1002
        })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/admin/family-links", new
        {
            FamilyUserId = familyUserId, StudentId = 2001
        })).StatusCode);

        var familyUser = new TestUser(email, password, "Family", 1);
        var family = await _host.AuthenticateAsync(familyUser);
        using var familyClient = family.Client;
        familyClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", family.AccessToken);
        using var visibleStudents = JsonDocument.Parse(await familyClient.GetStringAsync("/api/family/students"));
        Assert.Equal(new[] { 1001, 1002 }, visibleStudents.RootElement.EnumerateArray().Select(item => item.GetProperty("id").GetInt32()));

        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/admin/family-links/{familyUserId}/1001")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await familyClient.GetAsync("/api/family/students/1001/attendance")).StatusCode);
        foreach (var endpoint in new[] { "schedule", "grades", "records" })
            Assert.Equal(HttpStatusCode.NotFound,
                (await familyClient.GetAsync($"/api/family/students/1001/{endpoint}")).StatusCode);

        var student = await _host.AuthenticateAsync(_host.StudentOne);
        using var studentClient = student.Client;
        studentClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", student.AccessToken);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await studentClient.PostAsJsonAsync("/api/admin/family-links", new { FamilyUserId = familyUserId, StudentId = 1003 })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await familyClient.PostAsJsonAsync("/api/admin/family-links", new { FamilyUserId = familyUserId, StudentId = 1003 })).StatusCode);
    }

    [Fact]
    public async Task Authorization_code_pkce_redirect_antiforgery_and_token_claims_are_enforced()
    {
        using var anonymous = _host.CreateClient();
        var invalidRedirect = await anonymous.GetAsync(_host.BuildAuthorizationRequest(
            _host.AdminOne, "http://attacker.invalid/callback", includePkce: true));
        Assert.True((int)invalidRedirect.StatusCode >= 400);

        var missingPkce = await anonymous.GetAsync(_host.BuildAuthorizationRequest(
            _host.AdminOne, _host.RedirectUri, includePkce: false));
        Assert.True((int)missingPkce.StatusCode >= 400);

        var invalidVerifierSession = await _host.LoginToAuthorizationAsync(_host.AdminOne);
        using (invalidVerifierSession.Client)
        {
            var callbackResponse = await invalidVerifierSession.Client.GetAsync(invalidVerifierSession.AuthorizationPath);
            Assert.Equal(HttpStatusCode.Redirect, callbackResponse.StatusCode);
            var callbackUri = callbackResponse.Headers.Location is { IsAbsoluteUri: true } callbackAbsolute
                ? callbackAbsolute
                : new Uri(new Uri("https://localhost/"), callbackResponse.Headers.Location!);
            var code = QueryHelpers.ParseQuery(callbackUri.Query)["code"].ToString();
            var invalidExchange = await invalidVerifierSession.Client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code", ["client_id"] = "school-management-frontend",
                ["redirect_uri"] = _host.RedirectUri, ["code"] = code,
                ["code_verifier"] = _host.NewVerifierForTest()
            }));
            Assert.Equal(HttpStatusCode.BadRequest, invalidExchange.StatusCode);
        }

        var authorization = await _host.AuthorizeAndExchangeAsync(_host.AdminOne, verifyAntiforgery: true);
        Assert.Equal(authorization.State, authorization.CallbackState);
        Assert.False(string.IsNullOrWhiteSpace(authorization.AccessToken));

        using var api = authorization.Client;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization.AccessToken);
        using var me = JsonDocument.Parse(await api.GetStringAsync("/api/auth/me"));
        Assert.Equal("1", me.RootElement.GetProperty("schoolId").GetString());
        Assert.Contains("SchoolAdministrator", me.RootElement.GetProperty("roles").EnumerateArray().Select(item => item.GetString()));
    }

    [Fact]
    public async Task Locked_and_unconfirmed_users_cannot_receive_authorization_codes()
    {
        var unconfirmed = await _host.CreateUserAsync("Student", 1, confirmed: false);
        var unconfirmedClient = await _host.LoginToAuthorizationAsync(unconfirmed);
        using (unconfirmedClient.Client)
        {
            var response = await unconfirmedClient.Client.GetAsync(unconfirmedClient.AuthorizationPath);
            Assert.True(response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Redirect,
                $"An unconfirmed account must be rejected; received {(int)response.StatusCode}.");
            if (response.Headers.Location is { } unconfirmedLocation)
                Assert.DoesNotContain("code=", unconfirmedLocation.OriginalString, StringComparison.OrdinalIgnoreCase);
        }

        var locked = await _host.CreateUserAsync("Student", 1, confirmed: true);
        var lockedClient = await _host.LoginToAuthorizationAsync(locked);
        using var lockedHttpClient = lockedClient.Client;
        await _host.LockUserAsync(locked.Email);
        var lockedResponse = await lockedClient.Client.GetAsync(lockedClient.AuthorizationPath);
        Assert.True(lockedResponse.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Redirect,
            $"A locked account must be rejected; received {(int)lockedResponse.StatusCode}.");
        if (lockedResponse.Headers.Location is { } location)
            Assert.DoesNotContain("code=", location.OriginalString, StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class SecurityTestHost : IAsyncLifetime
{
    private const string OidcClientId = "school-management-frontend";
    private readonly string _storageRoot = Path.Combine(Path.GetTempPath(), "SchoolManagementSecurityTests-" + Guid.NewGuid().ToString("N"));
    private readonly SecurityTestApplicationFactory _factory;
    private bool _initialized;

    public SecurityTestHost()
    {
        AdminOne = new TestUser(NewEmail(), NewPassword(), "SchoolAdministrator", 1);
        AdminTwo = new TestUser(NewEmail(), NewPassword(), "SchoolAdministrator", 2);
        StudentOne = new TestUser(NewEmail(), NewPassword(), "Student", 1, studentId: 1001);
        StudentTwo = new TestUser(NewEmail(), NewPassword(), "Student", 1, studentId: 1002);
        TeacherOne = new TestUser(NewEmail(), NewPassword(), "Teacher", 1, teacherId: 1001);
        TeacherTwo = new TestUser(NewEmail(), NewPassword(), "Teacher", 1, teacherId: 1002);
        FamilyOne = new TestUser(NewEmail(), NewPassword(), "Family", 1);
        FamilyTwo = new TestUser(NewEmail(), NewPassword(), "Family", 1);
        FamilyOtherSchool = new TestUser(NewEmail(), NewPassword(), "Family", 2);
        _factory = new SecurityTestApplicationFactory(this);
    }

    public TestUser AdminOne { get; }
    public TestUser AdminTwo { get; }
    public TestUser StudentOne { get; }
    public TestUser StudentTwo { get; }
    public TestUser TeacherOne { get; }
    public TestUser TeacherTwo { get; }
    public TestUser FamilyOne { get; }
    public TestUser FamilyTwo { get; }
    public TestUser FamilyOtherSchool { get; }
    public string RedirectUri => "http://localhost:5173/auth/callback";
    internal string StorageRoot => _storageRoot;

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        using var startupClient = CreateClient();
        using (var validationScope = _factory.Services.CreateScope())
        {
            var applicationManager = validationScope.ServiceProvider.GetRequiredService<OpenIddict.Abstractions.IOpenIddictApplicationManager>();
            var registeredClient = await applicationManager.FindByClientIdAsync(OidcClientId);
            var clients = new List<string?>();
            await foreach (var application in applicationManager.ListAsync())
                clients.Add(await applicationManager.GetClientIdAsync(application));
            Assert.True(registeredClient is not null, $"Registered clients: {string.Join(",", clients)}");
        }
        await SeedAsync();
        _initialized = true;
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        if (Directory.Exists(_storageRoot)) Directory.Delete(_storageRoot, recursive: true);
        return Task.CompletedTask;
    }

    public HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost/"),
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    public string NewEmail() => $"security-{Guid.NewGuid():N}@example.invalid";

    public string NewPassword()
    {
        var random = Convert.ToHexString(RandomNumberGenerator.GetBytes(18));
        return $"T3st!{random}a";
    }

    public string NewVerifierForTest() => NewVerifier();

    public string BuildAuthorizationRequest(TestUser user, string redirectUri, bool includePkce)
    {
        var values = new Dictionary<string, string?>
        {
            ["client_id"] = OidcClientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid profile email roles api offline_access",
            ["state"] = "state-" + Guid.NewGuid().ToString("N"),
            ["nonce"] = "nonce-" + Guid.NewGuid().ToString("N")
        };
        if (includePkce)
        {
            var verifier = NewVerifier();
            values["code_challenge"] = CreateChallenge(verifier);
            values["code_challenge_method"] = "S256";
        }
        return QueryHelpers.AddQueryString("/connect/authorize", values);
    }

    public async Task<AuthorizationResult> AuthenticateAsync(TestUser user)
    {
        var result = await AuthorizeAndExchangeAsync(user, verifyAntiforgery: false);
        return result;
    }

    public async Task<AuthorizationResult> AuthorizeAndExchangeAsync(TestUser user, bool verifyAntiforgery)
    {
        var session = await LoginToAuthorizationAsync(user, verifyAntiforgery);
        var response = await session.Client.GetAsync(session.AuthorizationPath);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var callback = response.Headers.Location is { IsAbsoluteUri: true } absolute
            ? absolute
            : new Uri(new Uri("https://localhost/"), response.Headers.Location!);
        var callbackQuery = QueryHelpers.ParseQuery(callback.Query);
        var code = callbackQuery["code"].ToString();
        var callbackState = callbackQuery["state"].ToString();
        Assert.False(string.IsNullOrWhiteSpace(code));

        var tokenResponse = await session.Client.PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = OidcClientId,
            ["redirect_uri"] = RedirectUri,
            ["code"] = code,
            ["code_verifier"] = session.CodeVerifier
        }));
        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);
        using var tokenJson = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
        var accessToken = tokenJson.RootElement.GetProperty("access_token").GetString()!;
        return new AuthorizationResult(session.Client, session.State, callbackState, accessToken);
    }

    public async Task<LoginSession> LoginToAuthorizationAsync(TestUser user, bool verifyAntiforgery = false)
    {
        var client = CreateClient();
        var verifier = NewVerifier();
        var state = "state-" + Guid.NewGuid().ToString("N");
        var authorizationPath = QueryHelpers.AddQueryString("/connect/authorize", new Dictionary<string, string?>
        {
            ["client_id"] = OidcClientId,
            ["redirect_uri"] = RedirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid profile email roles api offline_access",
            ["state"] = state,
            ["nonce"] = "nonce-" + Guid.NewGuid().ToString("N"),
            ["code_challenge"] = CreateChallenge(verifier),
            ["code_challenge_method"] = "S256"
        });
        var challenge = await client.GetAsync(authorizationPath);
        Assert.True(challenge.StatusCode == HttpStatusCode.Redirect,
            $"Expected the OIDC login challenge redirect; received {(int)challenge.StatusCode}: {await challenge.Content.ReadAsStringAsync()}");
        var loginUri = challenge.Headers.Location is { IsAbsoluteUri: true } absoluteLogin
            ? absoluteLogin
            : new Uri(new Uri("https://localhost/"), challenge.Headers.Location!);
        var loginPage = await client.GetStringAsync(loginUri);
        var hiddenFields = ReadHiddenFields(loginPage);
        Assert.True(hiddenFields.TryGetValue("__RequestVerificationToken", out var antiforgery));
        Assert.True(hiddenFields.TryGetValue("ReturnUrl", out var returnUrl));

        if (verifyAntiforgery)
        {
            var rejected = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.Email"] = user.Email,
                ["Input.Password"] = user.Password,
                ["Input.RememberMe"] = "false",
                ["ReturnUrl"] = returnUrl!
            }));
            Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        }

        var login = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = user.Email,
            ["Input.Password"] = user.Password,
            ["Input.RememberMe"] = "false",
            ["ReturnUrl"] = returnUrl!,
            ["__RequestVerificationToken"] = antiforgery!
        }));
        Assert.True(login.StatusCode == HttpStatusCode.Redirect,
            $"Expected login redirect, received {(int)login.StatusCode}: {await login.Content.ReadAsStringAsync()}");
        var localReturnUrl = login.Headers.Location?.OriginalString ?? throw new InvalidOperationException("Login did not return to the authorization request.");
        return new LoginSession(client, localReturnUrl, verifier, state);
    }

    public async Task<TestUser> CreateUserAsync(string role, int schoolId, bool confirmed)
    {
        var user = new TestUser(NewEmail(), NewPassword(), role, schoolId);
        await AddIdentityUserAsync(user, confirmed);
        return user;
    }

    public async Task LockUserAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email) ?? throw new InvalidOperationException("Test user was not found.");
        var result = await users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(10));
        Assert.True(result.Succeeded);
    }

    private async Task SeedAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var context = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var adminSchool = await context.Schools.IgnoreQueryFilters().AnyAsync(school => school.Id == 2);
        if (!adminSchool)
        {
            context.Schools.Add(new School { Id = 2, Name = "Security Test School Two", CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
        }

        accessor.HttpContext = SeedHttpContext(1);
        context.Courses.Add(new Course { Id = 1001, Name = "School One Course" });
        context.Teachers.AddRange(
            new Teacher { Id = 1001, FirstName = "One", LastName = "Teacher", DocumentNumber = "1001", IsActive = true },
            new Teacher { Id = 1002, FirstName = "Other", LastName = "Teacher", DocumentNumber = "1002", IsActive = true });
        context.Subjects.Add(new Subject { Id = 1001, Name = "School One Subject" });
        context.TeacherSchedules.AddRange(
            new TeacherSchedule { Id = 1101, TeacherId = 1001, CourseId = 1001, SubjectId = 1001, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(9, 0, 0) },
            new TeacherSchedule { Id = 1102, TeacherId = 1002, CourseId = 1001, SubjectId = 1001, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 0, 0) });
        context.Students.AddRange(
            new Student { Id = 1001, FirstName = "Student", LastName = "One", DocumentNumber = "1101", CourseId = 1001 },
            new Student { Id = 1002, FirstName = "Student", LastName = "Two", DocumentNumber = "1102", CourseId = 1001 },
            new Student { Id = 1003, FirstName = "Student", LastName = "Three", DocumentNumber = "1103", CourseId = 1001 });
        context.Activities.AddRange(
            Activity(3001, 1, 1001, 1001, 1001, published: true),
            Activity(3002, 1, 1002, 1001, 1001, published: true),
            Activity(3003, 1, 1001, 1001, 1001, published: false));
        context.ActivityStudents.AddRange(
            Assignment(5001, 1, 3001, 1001), Assignment(5002, 1, 3001, 1002),
            Assignment(5004, 1, 3002, 1002), Assignment(5003, 1, 3003, 1001));
        context.Attendances.Add(new Attendance
        {
            Id = 8501, SchoolId = 1, StudentId = 1001, Date = DateTime.UtcNow.Date.AddDays(-1), Status = AttendanceStatus.Absent
        });
        context.StudentSubmissions.AddRange(
            Submission(6001, 1, 5001, 7001, "3001"),
            Submission(6002, 1, 5002, 7002, "3001"));
        context.ActivityMaterials.Add(new ActivityMaterial
        {
            Id = 8001, ActivityId = 3001, SchoolId = 1, Type = ActivityMaterialType.File,
            Name = "student material", FileName = "material.txt", StorageKey = StorageKey(3001, "material"), CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        accessor.HttpContext = SeedHttpContext(2);
        context.Courses.Add(new Course { Id = 2001, Name = "School Two Course" });
        context.Teachers.Add(new Teacher { Id = 2001, FirstName = "Other", LastName = "School Teacher", DocumentNumber = "2001", IsActive = true });
        context.Subjects.Add(new Subject { Id = 2001, Name = "School Two Subject" });
        context.TeacherSchedules.Add(new TeacherSchedule { Id = 2101, TeacherId = 2001, CourseId = 2001, SubjectId = 2001, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(9, 0, 0) });
        context.Students.AddRange(
            new Student { Id = 2001, FirstName = "Other", LastName = "School Student", DocumentNumber = "2101", CourseId = 2001 },
            new Student { Id = 2002, FirstName = "Other", LastName = "School Student Two", DocumentNumber = "2102", CourseId = 2001 });
        context.Activities.Add(Activity(4001, 2, 2001, 2001, 2001, published: true));
        context.ActivityStudents.Add(Assignment(9001, 2, 4001, 2001));
        context.Attendances.Add(new Attendance
        {
            Id = 8502, SchoolId = 2, StudentId = 2001, Date = DateTime.UtcNow.Date.AddDays(-1), Status = AttendanceStatus.Present
        });
        context.StudentSubmissions.Add(Submission(9001, 2, 9001, 9002, "4001"));
        context.ActivityMaterials.Add(new ActivityMaterial
        {
            Id = 8002, ActivityId = 4001, SchoolId = 2, Type = ActivityMaterialType.File,
            Name = "other school material", FileName = "other.txt", StorageKey = StorageKey(4001, "material"), CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        WriteStoredFile(3001, StorageKey(3001, "7001"), "student-one-file");
        WriteStoredFile(3001, StorageKey(3001, "7002"), "student-two-file");
        WriteStoredFile(3001, StorageKey(3001, "material"), "school-one-material");
        WriteStoredFile(4001, StorageKey(4001, "9002"), "other-school-file");
        WriteStoredFile(4001, StorageKey(4001, "material"), "other-school-material");

        accessor.HttpContext = null;
        await AddIdentityUserAsync(AdminOne, confirmed: true);
        await AddIdentityUserAsync(StudentOne, confirmed: true);
        await AddIdentityUserAsync(StudentTwo, confirmed: true);
        await AddIdentityUserAsync(TeacherOne, confirmed: true);
        await AddIdentityUserAsync(TeacherTwo, confirmed: true);
        await AddIdentityUserAsync(new TestUser(NewEmail(), NewPassword(), "Student", 2, studentId: 2001), confirmed: true);
        await AddIdentityUserAsync(FamilyOne, confirmed: true);
        await AddIdentityUserAsync(FamilyTwo, confirmed: true);
        await AddIdentityUserAsync(FamilyOtherSchool, confirmed: true);
        await AddIdentityUserAsync(AdminTwo, confirmed: true);
        await AddFamilyLinksAsync(FamilyOne, 1001, 1002);
        await AddFamilyLinksAsync(FamilyTwo, 1001);
        await AddFamilyLinksAsync(FamilyOtherSchool, 2001);
    }

    private async Task AddFamilyLinksAsync(TestUser family, params int[] studentIds)
    {
        using var scope = _factory.Services.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        accessor.HttpContext = SeedHttpContext(family.SchoolId);
        var context = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var user = await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(family.Email)
            ?? throw new InvalidOperationException("Family test account was not found.");
        context.FamilyStudentLinks.AddRange(studentIds.Select(studentId => new FamilyStudentLink
        {
            FamilyUserId = user.Id, StudentId = studentId, SchoolId = family.SchoolId,
            LinkedAt = DateTime.UtcNow, LinkedByUserId = "test-school-admin"
        }));
        await context.SaveChangesAsync();
        accessor.HttpContext = null;
    }

    private async Task AddIdentityUserAsync(TestUser account, bool confirmed)
    {
        using var scope = _factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = account.Email, Email = account.Email, EmailConfirmed = confirmed,
            SchoolId = account.SchoolId, StudentId = account.StudentId, TeacherId = account.TeacherId
        };
        var created = await users.CreateAsync(user, account.Password);
        Assert.True(created.Succeeded, string.Join("; ", created.Errors.Select(error => error.Description)));
        var roleAdded = await users.AddToRoleAsync(user, account.Role);
        Assert.True(roleAdded.Succeeded, string.Join("; ", roleAdded.Errors.Select(error => error.Description)));
    }

    private void WriteStoredFile(int activityId, string key, string contents)
    {
        var filePath = Path.Combine(_storageRoot, activityId.ToString(), key.Split('/')[1]);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, contents);
    }

    private static string StorageKey(int activityId, string suffix) => $"{activityId}/{StableGuid(suffix):N}";
    private static Guid StableGuid(string value) => new(SHA256.HashData(Encoding.UTF8.GetBytes(value + "-security-test"))[..16]);
    private static string NewVerifier() => Base64UrlTextEncoder.Encode(RandomNumberGenerator.GetBytes(32));
    private static string CreateChallenge(string verifier) => Base64UrlTextEncoder.Encode(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));

    private static Dictionary<string, string> ReadHiddenFields(string html)
    {
        var fields = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Match match in Regex.Matches(html, "<input\\b[^>]*>", RegexOptions.IgnoreCase))
        {
            var tag = match.Value;
            var name = Regex.Match(tag, "\\bname=\"([^\"]+)\"", RegexOptions.IgnoreCase);
            var value = Regex.Match(tag, "\\bvalue=\"([^\"]*)\"", RegexOptions.IgnoreCase);
            if (name.Success && value.Success)
                fields[name.Groups[1].Value] = WebUtility.HtmlDecode(value.Groups[1].Value);
        }
        return fields;
    }

    private static DefaultHttpContext SeedHttpContext(int schoolId) => new()
    {
        User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("school_id", schoolId.ToString()) }, "Seed"))
    };

    private static Activity Activity(int id, int schoolId, int teacherId, int courseId, int subjectId, bool published) => new()
    {
        Id = id, SchoolId = schoolId, TeacherId = teacherId, CourseId = courseId, SubjectId = subjectId,
        Title = "Test activity", Description = "Security test", DueAt = DateTime.UtcNow.AddDays(3),
        TargetType = ActivityTargetType.Course, CreatedAt = DateTime.UtcNow,
        PublishedAt = published ? DateTime.UtcNow : null, IsActive = true
    };

    private static ActivityStudent Assignment(int id, int schoolId, int activityId, int studentId) => new()
    {
        Id = id, SchoolId = schoolId, ActivityId = activityId, StudentId = studentId, AssignedAt = DateTime.UtcNow
    };

    private static StudentSubmission Submission(int id, int schoolId, int activityStudentId, int revisionId, string activityId) => new()
    {
        Id = id, SchoolId = schoolId, ActivityStudentId = activityStudentId, SubmittedAt = DateTime.UtcNow,
        Value = "test answer",
        Revisions = new List<SubmissionRevision>
        {
            new()
            {
                Id = revisionId, SchoolId = schoolId, Version = 1, FileName = "answer.txt", Value = "test answer",
                ContentType = "text/plain", FileSize = 10, StorageKey = StorageKey(int.Parse(activityId), revisionId.ToString()),
                SubmittedAt = DateTime.UtcNow, IsCurrent = true, CreatedAt = DateTime.UtcNow
            }
        }
    };
}

public sealed class SecurityTestApplicationFactory(SecurityTestHost host) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        var apiRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        builder.UseContentRoot(apiRoot);
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:Provider"] = "InMemory",
            ["Database:InMemoryName"] = "SchoolManagementSecurityTests-" + Guid.NewGuid().ToString("N"),
            ["Development:SchoolId"] = "1",
            ["Development:SchoolName"] = "Security Test School One",
            ["ActivityStorage:RootPath"] = host.StorageRoot,
        }));
    }
}

public sealed record TestUser(string Email, string Password, string Role, int SchoolId, int? studentId = null, int? teacherId = null)
{
    public int? StudentId { get; } = studentId;
    public int? TeacherId { get; } = teacherId;
}

public sealed record LoginSession(HttpClient Client, string AuthorizationPath, string CodeVerifier, string State);
public sealed record AuthorizationResult(HttpClient Client, string State, string CallbackState, string AccessToken);
