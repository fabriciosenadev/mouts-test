using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Infrastructure;

public sealed class PostgresIntegrationFixture : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5433;Database=developer_evaluation;Username=developer;Password=ev@luAt10n";

    public string ConnectionString { get; } =
        Environment.GetEnvironmentVariable("SALES_TEST_CONNECTION_STRING")
        ?? DefaultConnectionString;

    public DbContextOptions<DefaultContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<DefaultContext>()
            .UseNpgsql(ConnectionString, builder => builder.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM"))
            .Options;
    }

    public async Task InitializeAsync()
    {
        await using var context = new DefaultContext(CreateOptions());
        await context.Database.MigrateAsync();
        await ResetAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task ResetAsync()
    {
        await using var context = new DefaultContext(CreateOptions());
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"SaleItems\", \"Sales\" RESTART IDENTITY CASCADE;");
    }
}
