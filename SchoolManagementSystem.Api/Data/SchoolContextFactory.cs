using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SchoolManagementSystem.Api.Services;

namespace SchoolManagementSystem.Api.Data;

public sealed class SchoolContextFactory : IDesignTimeDbContextFactory<SchoolContext>
{
    public SchoolContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        var connectionString = configuration.GetConnectionString("SchoolDatabase");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Set ConnectionStrings__SchoolDatabase before running EF migration commands.");

        var options = new DbContextOptionsBuilder<SchoolContext>()
            .UseSqlServer(connectionString)
            .UseOpenIddict()
            .Options;
        return new SchoolContext(options, NoCurrentSchoolContext.Instance);
    }

    private sealed class NoCurrentSchoolContext : ICurrentSchoolContext
    {
        public static NoCurrentSchoolContext Instance { get; } = new();
        public int? SchoolId => null;
    }
}
