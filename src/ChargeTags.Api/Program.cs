using ChargeTags.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
// Retries on transient errors, e.g. while the serverless Azure SQL database is resuming from pause
builder.Services.AddDbContext<ChargeTagDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ChargeTags"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

var app = builder.Build();

// Creates the database and the table on startup if they do not exist.
// A production setup would use EF Core migrations instead.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ChargeTagDbContext>();
    dbContext.Database.EnsureCreated();
}

// Swagger UI is enabled in all environments so the API can be tried out after deployment
app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "ChargeTags API"));

app.MapControllers();


app.Run();
