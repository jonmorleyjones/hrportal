using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Portal.Api.Data;
using Portal.Api.Endpoints;
using Portal.Api.Middleware;
using Portal.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Portal API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Tenant context (scoped per request)
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// File storage
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddHostedService<OrphanFileCleanupService>();

// Configure Kestrel for larger file uploads (30 MB to allow buffer above 25 MB limit)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 30 * 1024 * 1024;
});

// Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT Secret not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "portal-api";

// Disable automatic claim type mapping so "sub" stays as "sub" (not mapped to ClaimTypes.NameIdentifier)
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtIssuer,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("role", "Admin"));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            // Allow any subdomain of localhost for development
            var uri = new Uri(origin);
            return uri.Host.EndsWith("localhost") || uri.Host == "localhost";
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Seed HR Consultant for development
    if (app.Environment.IsDevelopment())
    {
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await SeedHrConsultant(db, authService);
    }
}

static async Task SeedHrConsultant(AppDbContext db, IAuthService authService)
{
    const string testEmail = "hr@test.com";

    // Check if already exists
    if (await db.HrConsultants.AnyAsync(c => c.Email == testEmail))
    {
        return;
    }

    // Create the HR consultant
    var consultant = new Portal.Api.Models.HrConsultant
    {
        Id = Guid.NewGuid(),
        Email = testEmail,
        Name = "Test HR Consultant",
        PasswordHash = authService.HashPassword("password123"),
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };

    db.HrConsultants.Add(consultant);
    await db.SaveChangesAsync();

    // Assign to all existing tenants with full permissions
    var tenants = await db.Tenants.ToListAsync();
    foreach (var tenant in tenants)
    {
        var assignment = new Portal.Api.Models.HrConsultantTenantAssignment
        {
            Id = Guid.NewGuid(),
            HrConsultantId = consultant.Id,
            TenantId = tenant.Id,
            AssignedAt = DateTime.UtcNow,
            IsActive = true,
            CanManageRequestTypes = true,
            CanManageSettings = true,
            CanManageBranding = true,
            CanViewResponses = true
        };
        db.HrConsultantTenantAssignments.Add(assignment);
    }

    await db.SaveChangesAsync();
    Console.WriteLine($"Seeded HR Consultant: {testEmail} / password123");
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// Tenant middleware (before auth)
app.UseTenantMiddleware();

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapAuthEndpoints();
app.MapTenantEndpoints();
app.MapUserEndpoints();
app.MapDashboardEndpoints();
app.MapBillingEndpoints();
app.MapRequestEndpoints();
app.MapFileEndpoints();
app.MapHrConsultantEndpoints();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithTags("Health");

app.Run();

// Needed for WebApplicationFactory in tests
public partial class Program { }
