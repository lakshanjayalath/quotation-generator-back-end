using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using quotation_generator_back_end.Data;
using quotation_generator_back_end.Services;
// Added missing using statements for clarity
using Microsoft.AspNetCore.Builder; 
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System; 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

var builder = WebApplication.CreateBuilder(args); 

// Add services to the container.
builder.Services.AddControllers();

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

// Register Dashboard service (This part is correct)
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
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
              .AllowAnyMethod();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    // In development, the default pipeline includes the DeveloperExceptionPage
}
// 🛑 CRITICAL DEBUGGING STEP: Add DeveloperExceptionPage before UseHttpsRedirection 🛑
// This will force the application to show the full error details in the browser 
// when the 500 error occurs, even if app.Environment.IsDevelopment() is false.
app.UseDeveloperExceptionPage(); 

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowFrontend");

// Add Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();