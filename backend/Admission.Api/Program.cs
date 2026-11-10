using Admission.Api.Data;
using Admission.Api.Dtos;
using Admission.Api.Middleware;
using Admission.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Set with the ConnectionStrings__Default environment variable
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<ClassService>();
builder.Services.AddScoped<HoldService>();
builder.Services.AddHostedService<ExpiredHoldCleanupService>();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Return our own error format when the JSON body is invalid
        options.InvalidModelStateResponseFactory = _ =>
            new BadRequestObjectResult(new ErrorResponse("INVALID_REQUEST", "The request body is not valid."));
    });

// The frontend runs on another port, so allow cross-origin calls
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

await DatabaseSetup.MigrateAsync(app);

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

// needed so the test project can start the app
public partial class Program { }
