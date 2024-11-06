using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using Top10MediaApi.Services;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("Application", "Top10MediaApi")
    .WriteTo.Console()
    //.WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.WebHost.UseUrls("http://0.0.0.0:8080", "https://0.0.0.0:443");

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    options.JsonSerializerOptions.WriteIndented = true; // Optional: To make JSON output more readable
});

builder.Services.AddScoped<TmdbService>();
builder.Services.AddScoped<RawgService>();
builder.Services.AddScoped<MoviesService>();
builder.Services.AddScoped<TvShowsService>();
builder.Services.AddScoped<GamesService>();

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

// Add JWT authentication configuration in the builder
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
    };
});

builder.Services.AddAuthorization();

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

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
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

RecurringJob.AddOrUpdate<Top10TvShowsController>(
    recurringJobId: "ResetTop10MoviesJob",
    methodCall: controller => controller.ResetTop10TvShows(),
    cronExpression: Cron.Weekly,
    options: new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    }
);

RecurringJob.AddOrUpdate<Top10GamesController>(
    recurringJobId: "ResetTop10GamesJob",
    methodCall: controller => controller.ResetTop10Games(),
    cronExpression: Cron.Weekly,
    options: new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    }
);

app.MapControllers();

app.Run();
