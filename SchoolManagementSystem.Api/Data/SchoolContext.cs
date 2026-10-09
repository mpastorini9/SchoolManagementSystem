using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Api.Models;

namespace SchoolManagementSystem.Api.Data
{
    public class SchoolContext : DbContext
    {
        public SchoolContext(DbContextOptions<SchoolContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<TeacherSchedule> TeacherSchedules { get; set; }
        public DbSet<TeacherAttendance> TeacherAttendances { get; set; }

        public DbSet<Activity> Activities { get; set; }
        public DbSet<ActivityMaterial> ActivityMaterials { get; set; }
        public DbSet<ActivityStudent> ActivityStudents { get; set; }
        public DbSet<StudentSubmission> StudentSubmissions { get; set; }
        public DbSet<SubmissionRevision> SubmissionRevisions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ActivityStudent>()
                .HasIndex(item => new { item.ActivityId, item.StudentId })
                .IsUnique();

            modelBuilder.Entity<StudentSubmission>()
                .HasIndex(item => item.ActivityStudentId)
                .IsUnique();
        }
    }
}