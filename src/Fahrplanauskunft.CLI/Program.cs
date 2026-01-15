using Fahrplanauskunft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

AnsiConsole.Write(
    new FigletText("Fahrplanauskunft")
        .Color(Color.Blue));

AnsiConsole.MarkupLine("[grey]Public transit journey planner[/]");

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");

// Build service provider with DbContext
var services = new ServiceCollection();
services.AddDbContext<FahrplanDbContext>(options =>
    options.UseNpgsql(connectionString));

await using var serviceProvider = services.BuildServiceProvider();

// Apply pending migrations on startup
try
{
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<FahrplanDbContext>();

    AnsiConsole.MarkupLine("[yellow]Checking for pending database migrations...[/]");

    var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

    if (pendingMigrations.Count > 0)
    {
        AnsiConsole.MarkupLine($"[yellow]Applying {pendingMigrations.Count} pending migration(s)...[/]");
        await dbContext.Database.MigrateAsync();
        AnsiConsole.MarkupLine("[green]Migrations applied successfully.[/]");
    }
    else
    {
        AnsiConsole.MarkupLine("[green]Database is up to date.[/]");
    }
}
catch (Exception ex) when (ex is Npgsql.NpgsqlException or InvalidOperationException)
{
    AnsiConsole.MarkupLine($"[red]Database error: {ex.Message}[/]");
    AnsiConsole.MarkupLine("[grey]Ensure PostgreSQL is running and the connection string is correct.[/]");
    return 1;
}

return 0;
