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

        

        /*protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course>().HasData(
                new Course { Id = 1, Name = "2°D" },
                new Course { Id = 2, Name = "3°B" }
            );

            modelBuilder.Entity<Student>().HasData(
                new Student { Id = 1, FirstName = "Juan", LastName = "Perez", DocumentNumber = "123", CourseId = 1 },
                new Student { Id = 2, FirstName = "Pedro", LastName = "Gomez", DocumentNumber = "456", CourseId = 1 },
                new Student { Id = 3, FirstName = "Lucia", LastName = "Lopez", DocumentNumber = "789", CourseId = 2 },
                new Student { Id = 4, FirstName = "Maria", LastName = "Gimenez", DocumentNumber = "5789", CourseId = 1 },
                new Student { Id = 5, FirstName = "Jose", LastName = "Perez", DocumentNumber = "4789", CourseId = 2 },
                new Student { Id = 6, FirstName = "Ana", LastName = "Lopez", DocumentNumber = "7689", CourseId = 2 },
                new Student { Id = 7, FirstName = "Silvia", LastName = "Pastoreli", DocumentNumber = "7289", CourseId = 2 }
            );
        }*/
    }
}