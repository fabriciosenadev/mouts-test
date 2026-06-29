using Ambev.DeveloperEvaluation.ORM;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Infrastructure;

public class SalesApiFactory : WebApplicationFactory<Ambev.DeveloperEvaluation.WebApi.Program>, IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5433;Database=developer_evaluation;Username=developer;Password=ev@luAt10n";

    public string ConnectionString { get; } =
        Environment.GetEnvironmentVariable("SALES_TEST_CONNECTION_STRING")
        ?? DefaultConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString
            });
        });
    }

    public async Task InitializeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        await context.Database.MigrateAsync();
        await ResetAsync();
    }

    public new async Task DisposeAsync()
    {
        await ResetAsync();
        Dispose();
    }

    public async Task ResetAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"SaleItems\", \"Sales\" RESTART IDENTITY CASCADE;");
    }
}
