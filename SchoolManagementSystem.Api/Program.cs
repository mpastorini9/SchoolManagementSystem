using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using SchoolManagementSystem.Api.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var isDevelopment = builder.Environment.IsDevelopment();
var databaseProvider = builder.Configuration["Database:Provider"] ?? (isDevelopment ? "InMemory" : null);
var useInMemory = string.Equals(databaseProvider, "InMemory", StringComparison.OrdinalIgnoreCase);
var connectionString = builder.Configuration.GetConnectionString("SchoolDatabase");
if (!isDevelopment && !string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("Database:Provider must be SqlServer outside Development.");
if (isDevelopment && !useInMemory && !string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("Database:Provider must be InMemory or SqlServer.");
if (useInMemory && !isDevelopment)
    throw new InvalidOperationException("The InMemory database provider is available only in Development.");
if (!useInMemory && string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:SchoolDatabase is required for the configured durable database provider.");

var issuer = builder.Configuration["Authentication:Issuer"] ?? (isDevelopment ? "https://localhost:7182/" : null);
var frontendOrigin = builder.Configuration["Authentication:FrontendOrigin"] ?? (isDevelopment ? "http://localhost:5173" : null);
var clientId = builder.Configuration["Authentication:ClientId"] ?? (isDevelopment ? "school-management-frontend" : null);
var allowedOrigins = builder.Configuration.GetSection("Authentication:AllowedOrigins").Get<string[]>()
    ?? (isDevelopment ? new[] { "http://localhost:5173" } : Array.Empty<string>());
if (string.IsNullOrWhiteSpace(issuer) || !Uri.TryCreate(issuer, UriKind.Absolute, out var issuerUri) ||
    (!isDevelopment && issuerUri.Scheme != Uri.UriSchemeHttps))
    throw new InvalidOperationException("Authentication:Issuer must be configured as an absolute HTTPS URI outside Development.");
if (string.IsNullOrWhiteSpace(frontendOrigin) || !Uri.TryCreate(frontendOrigin, UriKind.Absolute, out var frontendUri) ||
    string.IsNullOrWhiteSpace(clientId) || allowedOrigins.Length == 0 ||
    allowedOrigins.Any(origin => !Uri.TryCreate(origin, UriKind.Absolute, out var parsedOrigin) ||
        (!isDevelopment && parsedOrigin.Scheme != Uri.UriSchemeHttps)))
    throw new InvalidOperationException("Authentication:FrontendOrigin, ClientId and valid AllowedOrigins are required.");
if (!isDevelopment && frontendUri.Scheme != Uri.UriSchemeHttps)
    throw new InvalidOperationException("Authentication:FrontendOrigin must use HTTPS outside Development.");

var bootstrapEmail = builder.Configuration["Authentication:BootstrapAdmin:Email"];
var bootstrapPassword = builder.Configuration["Authentication:BootstrapAdmin:Password"];
var bootstrapSchoolValue = builder.Configuration["Authentication:BootstrapAdmin:SchoolId"];
var bootstrapConfigured = !string.IsNullOrWhiteSpace(bootstrapEmail) || !string.IsNullOrWhiteSpace(bootstrapPassword) ||
    !string.IsNullOrWhiteSpace(bootstrapSchoolValue);
if (bootstrapConfigured && (string.IsNullOrWhiteSpace(bootstrapEmail) || string.IsNullOrWhiteSpace(bootstrapPassword) ||
    !int.TryParse(bootstrapSchoolValue, out var bootstrapSchoolId) || bootstrapSchoolId <= 0))
    throw new InvalidOperationException("Bootstrap administrator configuration requires Email, Password and a positive SchoolId.");

string? signingPath = null;
string? signingPassword = null;
string? encryptionPath = null;
string? encryptionPassword = null;
string? dataProtectionKeyRingPath = null;
X509Certificate2? persistentSigningCertificate = null;
X509Certificate2? persistentEncryptionCertificate = null;
if (!isDevelopment)
{
    signingPath = builder.Configuration["Authentication:Certificates:SigningPath"];
    signingPassword = builder.Configuration["Authentication:Certificates:SigningPassword"];
    encryptionPath = builder.Configuration["Authentication:Certificates:EncryptionPath"];
    encryptionPassword = builder.Configuration["Authentication:Certificates:EncryptionPassword"];
    dataProtectionKeyRingPath = builder.Configuration["Authentication:DataProtection:KeyRingPath"];
    if (string.IsNullOrWhiteSpace(signingPath) || string.IsNullOrWhiteSpace(signingPassword) ||
        string.IsNullOrWhiteSpace(encryptionPath) || string.IsNullOrWhiteSpace(encryptionPassword) ||
        !File.Exists(signingPath) || !File.Exists(encryptionPath) ||
        string.IsNullOrWhiteSpace(dataProtectionKeyRingPath) || !Directory.Exists(dataProtectionKeyRingPath))
        throw new InvalidOperationException("Persistent OpenID Connect certificates and an existing Data Protection key-ring directory must be configured outside Development.");
    try
    {
        persistentSigningCertificate = new X509Certificate2(signingPath, signingPassword, X509KeyStorageFlags.EphemeralKeySet);
        persistentEncryptionCertificate = new X509Certificate2(encryptionPath, encryptionPassword, X509KeyStorageFlags.EphemeralKeySet);
    }
    catch (CryptographicException exception)
    {
        throw new InvalidOperationException("The configured authentication certificates could not be loaded.", exception);
    }
    if (!persistentSigningCertificate.HasPrivateKey || !persistentEncryptionCertificate.HasPrivateKey ||
        persistentSigningCertificate.NotAfter <= DateTime.UtcNow || persistentEncryptionCertificate.NotAfter <= DateTime.UtcNow)
        throw new InvalidOperationException("The configured authentication certificates must be unexpired and contain their private keys.");

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyRingPath))
        .ProtectKeysWithCertificate(persistentEncryptionCertificate)
        .SetApplicationName("SchoolManagementSystem");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentSchoolContext>();
builder.Services.AddScoped<ICurrentSchoolContext>(services => services.GetRequiredService<CurrentSchoolContext>());
builder.Services.AddDbContext<SchoolContext>(options =>
{
    if (useInMemory)
        options.UseInMemoryDatabase(builder.Configuration["Database:InMemoryName"] ?? "SchoolDb");
    else
        options.UseSqlServer(connectionString);
    options.UseOpenIddict();
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
    .AddEntityFrameworkStores<SchoolContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddRazorPages();
var oidc = builder.Services.AddOpenIddict();
oidc.AddCore(options => options.UseEntityFrameworkCore().UseDbContext<SchoolContext>());
oidc.AddServer(options =>
{
    options.SetIssuer(new Uri(issuer!, UriKind.Absolute));
    options.SetAuthorizationEndpointUris("connect/authorize");
    options.SetTokenEndpointUris("connect/token");
    options.SetUserInfoEndpointUris("connect/userinfo");
    options.SetEndSessionEndpointUris("connect/logout");
    options.AllowAuthorizationCodeFlow().AllowRefreshTokenFlow().RequireProofKeyForCodeExchange();
    options.RegisterScopes(Scopes.OpenId, Scopes.Profile, Scopes.Email, Scopes.Roles, Scopes.OfflineAccess, "api");
    if (isDevelopment)
    {
        options.AddEphemeralSigningKey().AddEphemeralEncryptionKey();
    }
    else
    {
        options.AddSigningCertificate(persistentSigningCertificate!);
        options.AddEncryptionCertificate(persistentEncryptionCertificate!);
    }
    options.UseAspNetCore()
        .EnableAuthorizationEndpointPassthrough()
        .EnableUserInfoEndpointPassthrough()
        .EnableEndSessionEndpointPassthrough();
});
oidc.AddValidation(options =>
{
    options.UseLocalServer();
    options.UseAspNetCore();
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

builder.Services.AddAuthorization(options =>
{
    var apiPolicy = new AuthorizationPolicyBuilder(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser().Build();
    options.DefaultPolicy = apiPolicy;
    options.FallbackPolicy = apiPolicy;
});

builder.Services.AddSingleton<ActivityFileStorage>();


// ==========================
// CORS
// ==========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SchoolContext>();
    if (useInMemory)
    {
        context.Database.EnsureCreated();
        var configuredDevelopmentSchoolId = builder.Configuration.GetValue<int?>("Development:SchoolId") ?? 1;
        if (!context.Schools.IgnoreQueryFilters().Any(school => school.Id == configuredDevelopmentSchoolId))
        {
            context.Schools.Add(new School
            {
                Id = configuredDevelopmentSchoolId,
                Name = builder.Configuration["Development:SchoolName"] ?? "Development School",
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();
        }
        scope.ServiceProvider.GetRequiredService<CurrentSchoolContext>().SetForInitialization(configuredDevelopmentSchoolId);
    }
    else if (context.Database.GetPendingMigrations().Any())
    {
        throw new InvalidOperationException(
            "The database has pending migrations. Apply them with 'dotnet ef database update' before starting the API.");
    }

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { SchoolRoles.SchoolAdministrator, SchoolRoles.Teacher, SchoolRoles.Student, SchoolRoles.Family })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var roleCreated = await roleManager.CreateAsync(new IdentityRole(role));
            if (!roleCreated.Succeeded) throw new InvalidOperationException($"Could not create the {role} role.");
        }
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    if (bootstrapConfigured)
    {
        var adminSchoolId = int.Parse(bootstrapSchoolValue!, System.Globalization.CultureInfo.InvariantCulture);
        if (!context.Schools.IgnoreQueryFilters().Any(school => school.Id == adminSchoolId))
            throw new InvalidOperationException("The configured bootstrap administrator SchoolId does not exist.");
        var admin = await userManager.FindByEmailAsync(bootstrapEmail!);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = bootstrapEmail, Email = bootstrapEmail, EmailConfirmed = true, SchoolId = adminSchoolId
            };
            var created = await userManager.CreateAsync(admin, bootstrapPassword!);
            if (!created.Succeeded) throw new InvalidOperationException("Could not create the configured bootstrap administrator.");
            var roleAdded = await userManager.AddToRoleAsync(admin, SchoolRoles.SchoolAdministrator);
            if (!roleAdded.Succeeded) throw new InvalidOperationException("Could not assign the administrator role to the configured bootstrap account.");
        }
        else if (admin.SchoolId != adminSchoolId || !await userManager.IsInRoleAsync(admin, SchoolRoles.SchoolAdministrator))
            throw new InvalidOperationException("The configured bootstrap email belongs to an account without the administrator role.");
    }

    var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
    if (await appManager.FindByClientIdAsync(clientId) is null)
    {
        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId!,
            DisplayName = "School Management frontend",
            ClientType = ClientTypes.Public,
            RedirectUris = { new Uri($"{frontendOrigin!.TrimEnd('/')}/auth/callback") },
            PostLogoutRedirectUris = { new Uri($"{frontendOrigin.TrimEnd('/')}/") },
            Permissions =
            {
                Permissions.Endpoints.Authorization, Permissions.Endpoints.Token, Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode, Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code, Permissions.Scopes.Email, Permissions.Scopes.Profile,
                Permissions.Scopes.Roles, Permissions.Prefixes.Scope + "api", Permissions.Prefixes.Scope + Scopes.OfflineAccess
            },
            Requirements = { Requirements.Features.ProofKeyForCodeExchange }
        });
    }

    var developmentSchoolId = builder.Configuration.GetValue<int?>("Development:SchoolId") ?? 1;
    if (useInMemory && !context.Courses.IgnoreQueryFilters().Any(course => course.SchoolId == developmentSchoolId))
    {
        context.Courses.AddRange(
            new Course { Id = 1, SchoolId = developmentSchoolId, Name = "2°D" },
            new Course { Id = 2, SchoolId = developmentSchoolId, Name = "2°C" },
            new Course { Id = 3, SchoolId = developmentSchoolId, Name = "3°B" }
        );

        context.Subjects.AddRange(
            new Subject { Id = 1, SchoolId = developmentSchoolId, Name = "Matematica" },
            new Subject { Id = 2, SchoolId = developmentSchoolId, Name = "Lengua" },
            new Subject { Id = 3, SchoolId = developmentSchoolId, Name = "Ciencias" }
        );

        context.Students.AddRange(
            new Student
            {
                Id = 1,
                SchoolId = developmentSchoolId,
                FirstName = "Juan",
                LastName = "Perez",
                DocumentNumber = "123",
                CourseId = 1
            },
            new Student
            {
                Id = 2,
                SchoolId = developmentSchoolId,
                FirstName = "Pedro",
                LastName = "Gomez",
                DocumentNumber = "456",
                CourseId = 1
            },
            new Student
            {
                Id = 3,
                SchoolId = developmentSchoolId,
                FirstName = "Lucia",
                LastName = "Lopez",
                DocumentNumber = "789",
                CourseId = 2
            },
            new Student
            {
                Id = 4,
                SchoolId = developmentSchoolId,
                FirstName = "Maria",
                LastName = "Gimenez",
                DocumentNumber = "5789",
                CourseId = 1
            },
            new Student
            {
                Id = 5,
                SchoolId = developmentSchoolId,
                FirstName = "Jose",
                LastName = "Perez",
                DocumentNumber = "4789",
                CourseId = 2
            },
            new Student
            {
                Id = 6,
                SchoolId = developmentSchoolId,
                FirstName = "Ana",
                LastName = "Lopez",
                DocumentNumber = "7689",
                CourseId = 2
            },
            new Student
            {
                Id = 7,
                SchoolId = developmentSchoolId,
                FirstName = "Silvia",
                LastName = "Pastoreli",
                DocumentNumber = "7289",
                CourseId = 2
            },
            new Student
            {
                Id = 8,
                SchoolId = developmentSchoolId,
                FirstName = "Romina",
                LastName = "Sanchez",
                DocumentNumber = "8289",
                CourseId = 3
            },
            new Student
            {
                Id = 9,
                SchoolId = developmentSchoolId,
                FirstName = "Manuel",
                LastName = "Paez",
                DocumentNumber = "70289",
                CourseId = 3
            },
            new Student
            {
                Id = 10,
                SchoolId = developmentSchoolId,
                FirstName = "Samanta",
                LastName = "Farjat",
                DocumentNumber = "720589",
                CourseId = 3
            }
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


// ==========================
// CORS
// ==========================
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();

public partial class Program { }
