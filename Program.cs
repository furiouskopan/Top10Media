using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Top10MediaApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:8080");

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    options.JsonSerializerOptions.WriteIndented = true; // Optional: To make JSON output more readable
});

builder.Services.AddScoped<TmdbService>();
builder.Services.AddScoped<MoviesService>();

// Get connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configure the DbContext to use PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHttpClient();

// Register Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(config => config.UseMemoryStorage());
builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Disable HTTPS redirection in production
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable Swagger for development and production environments
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Top10Media API V1");
        c.RoutePrefix = string.Empty; // This makes Swagger available at the root
    });
}

app.UseRouting();

app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");
RecurringJob.AddOrUpdate<Top10MoviesController>(
    recurringJobId: "ResetTop10MoviesJob",
    methodCall: controller => controller.ResetTop10Movies(),
    cronExpression: Cron.Weekly,
    options: new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    }
);

app.MapControllers();

app.Run();
