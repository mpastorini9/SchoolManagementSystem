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
    }
}