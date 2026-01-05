using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using quotation_generator_back_end.Data;
using quotation_generator_back_end.Services;
using quotation_generator_back_end.Models;
using quotation_generator_back_end.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Access HttpContext inside services (e.g., activity logging)
builder.Services.AddHttpContextAccessor();

// Add Entity Framework Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Register report services
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportExportService, ReportExportService>();

// Register Activity Logger service
builder.Services.AddScoped<IActivityLogger, ActivityLogger>();

// Register Dashboard service
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
if (!jwtSettings.Exists())
{
    throw new InvalidOperationException("JWT settings section is missing in appsettings.json");
}

var secretKey = jwtSettings["SecretKey"];
if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new InvalidOperationException("JWT SecretKey not configured");
}

var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Add CORS policy for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-Total-Count", "X-Page", "X-Page-Size", "X-Total-Pages");
    });
});

// OpenAPI / Swagger
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed Admin User
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!context.Users.Any(u => u.Role == "Admin"))
    {
        context.Users.Add(new User
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@example.com",
            PasswordHash = PasswordHelper.HashPassword("Admin@123"),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TwoFactorAuth = false,
            LoginNotification = true,
            TaskAssignNotification = true,
            DisableRecurringPaymentNotification = false
        });
        context.SaveChanges();
        Console.WriteLine("Default admin user created: admin@example.com / Admin@123");
    }
}

app.Run();
