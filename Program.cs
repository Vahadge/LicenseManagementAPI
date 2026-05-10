using LicenseManagementAPI.Application.Interfaces;
using LicenseManagementAPI.Application.Services;
using LicenseManagementAPI.Common.Middleware;
using LicenseManagementAPI.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "License Management API",
        Version = "v1",
        Description = "Doctor License Management API for a Medical SaaS platform"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IDoctorService, DoctorService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "License Management API v1"));
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("AllowFrontend");

// Skip HTTPS redirect in development — the HTTP→HTTPS redirect breaks CORS
// preflight requests because browsers don't follow redirects for cross-origin calls.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();
app.Run();
