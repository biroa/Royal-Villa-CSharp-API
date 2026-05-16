using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using RoyalVillaApi.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi(options =>
{
    // Align ApiExplorer GroupName with document name (.NET 10 matching is case-sensitive by default).
    options.ShouldInclude = description =>
        string.IsNullOrEmpty(description.GroupName)
        || string.Equals(description.GroupName, options.DocumentName, StringComparison.OrdinalIgnoreCase);

    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var httpContextAccessor = context.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
        if (httpContextAccessor.HttpContext is not { } httpContext)
        {
            return Task.CompletedTask;
        }

        var origin = $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}";
        document.Servers = [new OpenApiServer { Url = origin }];
        return Task.CompletedTask;
    });
});

var app = builder.Build();
await SeedDataAsync(app);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithDynamicBaseServerUrl(true));
    app.MapGet("/", () => Results.Redirect("/scalar"));
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

/// <summary>
///     Seed the database with initial data.
/// </summary>
/// <param name="app">The web application.</param>
/// <returns>A task representing the asynchronous operation.</returns>
static async Task SeedDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}