using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SchoolManagementSystem.Api.Models;
using SchoolManagementSystem.Api.Services;

namespace SchoolManagementSystem.Api.Data
{
    public class SchoolContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ICurrentSchoolContext _currentSchool;

        public SchoolContext(DbContextOptions<SchoolContext> options, ICurrentSchoolContext currentSchool) : base(options)
        {
            _currentSchool = currentSchool;
        }

        public DbSet<School> Schools { get; set; }

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
        public DbSet<FamilyStudentLink> FamilyStudentLinks { get; set; }
        public DbSet<AcademicEvaluation> AcademicEvaluations { get; set; }
        public DbSet<StudentGrade> StudentGrades { get; set; }
        public DbSet<StudentRecordEntry> StudentRecordEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>().HasOne(user => user.School).WithMany()
                .HasForeignKey(user => user.SchoolId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<Teacher>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<Course>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<Subject>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<Attendance>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<TeacherSchedule>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<TeacherAttendance>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<Activity>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<ActivityStudent>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<ActivityMaterial>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<StudentSubmission>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<SubmissionRevision>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<FamilyStudentLink>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<AcademicEvaluation>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<StudentGrade>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);
            modelBuilder.Entity<StudentRecordEntry>().HasQueryFilter(item => _currentSchool.SchoolId.HasValue && item.SchoolId == _currentSchool.SchoolId.Value);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                         .Where(entityType => typeof(ISchoolScoped).IsAssignableFrom(entityType.ClrType)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(School))
                    .WithMany()
                    .HasForeignKey(nameof(ISchoolScoped.SchoolId))
                    .OnDelete(DeleteBehavior.Restrict);
            }

            modelBuilder.Entity<ActivityStudent>()
                .HasIndex(item => new { item.ActivityId, item.StudentId })
                .IsUnique();

            modelBuilder.Entity<StudentSubmission>()
                .HasIndex(item => item.ActivityStudentId)
                .IsUnique();

            modelBuilder.Entity<SubmissionRevision>()
                .HasIndex(item => new { item.StudentSubmissionId, item.Version })
                .IsUnique();

            modelBuilder.Entity<Student>().HasAlternateKey(student => new { student.Id, student.SchoolId });
            modelBuilder.Entity<Teacher>().HasAlternateKey(teacher => new { teacher.Id, teacher.SchoolId });
            modelBuilder.Entity<ApplicationUser>().HasAlternateKey(user => new { user.Id, user.SchoolId });
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(user => user.Student)
                .WithOne(student => student.User)
                .HasPrincipalKey<Student>(student => new { student.Id, student.SchoolId })
                .HasForeignKey<ApplicationUser>(user => new { user.StudentId, user.SchoolId })
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(user => user.Teacher)
                .WithOne(teacher => teacher.User)
                .HasPrincipalKey<Teacher>(teacher => new { teacher.Id, teacher.SchoolId })
                .HasForeignKey<ApplicationUser>(user => new { user.TeacherId, user.SchoolId })
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FamilyStudentLink>()
                .HasIndex(link => new { link.FamilyUserId, link.StudentId, link.SchoolId })
                .IsUnique();
            modelBuilder.Entity<FamilyStudentLink>()
                .HasOne(link => link.FamilyUser)
                .WithMany(user => user.FamilyStudentLinks)
                .HasPrincipalKey(user => new { user.Id, user.SchoolId })
                .HasForeignKey(link => new { link.FamilyUserId, link.SchoolId })
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FamilyStudentLink>()
                .HasOne(link => link.Student)
                .WithMany(student => student.FamilyStudentLinks)
                .HasPrincipalKey(student => new { student.Id, student.SchoolId })
                .HasForeignKey(link => new { link.StudentId, link.SchoolId })
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>().HasAlternateKey(course => new { course.Id, course.SchoolId });
            modelBuilder.Entity<Subject>().HasAlternateKey(subject => new { subject.Id, subject.SchoolId });
            modelBuilder.Entity<AcademicEvaluation>().HasAlternateKey(evaluation => new { evaluation.Id, evaluation.SchoolId });
            modelBuilder.Entity<AcademicEvaluation>().HasOne(evaluation => evaluation.Course)
                .WithMany().HasPrincipalKey(course => new { course.Id, course.SchoolId })
                .HasForeignKey(evaluation => new { evaluation.CourseId, evaluation.SchoolId }).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AcademicEvaluation>().HasOne(evaluation => evaluation.Subject)
                .WithMany().HasPrincipalKey(subject => new { subject.Id, subject.SchoolId })
                .HasForeignKey(evaluation => new { evaluation.SubjectId, evaluation.SchoolId }).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AcademicEvaluation>().HasOne(evaluation => evaluation.Teacher)
                .WithMany().HasPrincipalKey(teacher => new { teacher.Id, teacher.SchoolId })
                .HasForeignKey(evaluation => new { evaluation.TeacherId, evaluation.SchoolId }).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StudentGrade>().HasOne(grade => grade.Evaluation)
                .WithMany(evaluation => evaluation.Grades).HasPrincipalKey(evaluation => new { evaluation.Id, evaluation.SchoolId })
                .HasForeignKey(grade => new { EvaluationId = grade.EvaluationId, grade.SchoolId }).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StudentGrade>().HasOne(grade => grade.Student)
                .WithMany().HasPrincipalKey(student => new { student.Id, student.SchoolId })
                .HasForeignKey(grade => new { grade.StudentId, grade.SchoolId }).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StudentGrade>().HasIndex(grade => new { grade.EvaluationId, grade.StudentId, grade.SchoolId }).IsUnique();
            modelBuilder.Entity<StudentGrade>().Property(grade => grade.Value).HasMaxLength(200).IsRequired();
            modelBuilder.Entity<StudentRecordEntry>().HasOne(entry => entry.Student)
                .WithMany().HasPrincipalKey(student => new { student.Id, student.SchoolId })
                .HasForeignKey(entry => new { entry.StudentId, entry.SchoolId }).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StudentRecordEntry>().HasIndex(entry => new { entry.StudentId, entry.SchoolId, entry.RecordedAt });
            modelBuilder.Entity<StudentRecordEntry>().Property(entry => entry.Title).HasMaxLength(160).IsRequired();
        }

        public override int SaveChanges()
        {
            ApplySchoolScope();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplySchoolScope();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplySchoolScope();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplySchoolScope();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplySchoolScope()
        {
            var currentSchoolId = _currentSchool.SchoolId;
            foreach (var entry in ChangeTracker.Entries<ISchoolScoped>())
            {
                if (!currentSchoolId.HasValue)
                    throw new InvalidOperationException("A school must be selected for the new record.");

                if (entry.State == EntityState.Added && currentSchoolId.HasValue)
                {
                    entry.Entity.SchoolId = currentSchoolId.Value;
                }

                if (entry.State is EntityState.Modified or EntityState.Deleted &&
                    (entry.Entity.SchoolId != currentSchoolId.Value ||
                     (int)entry.Property(nameof(ISchoolScoped.SchoolId)).OriginalValue! != currentSchoolId.Value))
                    throw new InvalidOperationException("A record from another school cannot be changed.");
            }
        }
    }
}
