using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignalChat.Backend.Database;
using Testcontainers.PostgreSql;

namespace SignalChat.Backend.Tests.Infrastructure;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("signalchat_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:ChatDb"] = _postgres.GetConnectionString(),
                ["Jwt:SecretKey"] = "TestSecretKey_MustBe32PlusCharsLong!",
                ["Jwt:Issuer"] = "SignalChat",
                ["Jwt:Audience"] = "SignalChat",
                ["Jwt:AccessTokenExpiryMinutes"] = "15",
                ["Jwt:RefreshTokenExpiryDays"] = "30"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ChatDbContext>))
                .ToList();

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            services.AddDbContext<ChatDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _postgres.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        await db.Messages.ExecuteDeleteAsync();
        await db.Users.ExecuteDeleteAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
