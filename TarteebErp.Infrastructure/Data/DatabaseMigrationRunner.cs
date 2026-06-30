using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TarteebErp.Infrastructure.Data;

public interface IDatabaseMigrationRunner
{
    void MigrateUp();
}

public class DatabaseMigrationRunner : IDatabaseMigrationRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMigrationRunner> _logger;

    public DatabaseMigrationRunner(IServiceProvider serviceProvider, ILogger<DatabaseMigrationRunner> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void MigrateUp()
    {
        try
        {
            _logger.LogInformation("Starting database migration...");
            using var scope = _serviceProvider.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
            _logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database migration!");
            throw;
        }
    }
}
