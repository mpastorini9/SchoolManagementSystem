using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(
                        new System.Text.Json.Serialization.JsonStringEnumConverter());
                });
builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseInMemoryDatabase("SchoolDb"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SchoolContext>();
    
    if (!context.Courses.Any())
    {
        context.Courses.AddRange(
            new Course { Id = 1, Name = "2°D" },
            new Course { Id = 2, Name = "3°B" }
        );

        context.Students.AddRange(
            new Student { Id = 1, FirstName = "Juan", LastName = "Perez", DocumentNumber = "123", CourseId = 1 },
            new Student { Id = 2, FirstName = "Pedro", LastName = "Gomez", DocumentNumber = "456", CourseId = 1 },
            new Student { Id = 3, FirstName = "Lucia", LastName = "Lopez", DocumentNumber = "789", CourseId = 2 },
            new Student { Id = 4, FirstName = "Maria", LastName = "Gimenez", DocumentNumber = "5789", CourseId = 1 },
            new Student { Id = 5, FirstName = "Jose", LastName = "Perez", DocumentNumber = "4789", CourseId = 2 },
            new Student { Id = 6, FirstName = "Ana", LastName = "Lopez", DocumentNumber = "7689", CourseId = 2 },
            new Student { Id = 7, FirstName = "Silvia", LastName = "Pastoreli", DocumentNumber = "7289", CourseId = 2 }
        );
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

