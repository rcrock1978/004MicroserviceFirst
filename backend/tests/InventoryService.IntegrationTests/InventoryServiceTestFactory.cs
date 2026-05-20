using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using InventoryService.Infrastructure;

namespace InventoryService.IntegrationTests;

public class InventoryServiceTestFactory : WebApplicationFactory<Program>
{
    private readonly string _postgresConnectionString;
    private readonly string _rabbitMqHost;

    public InventoryServiceTestFactory(string postgresConnectionString, string rabbitMqHost)
    {
        _postgresConnectionString = postgresConnectionString;
        _rabbitMqHost = rabbitMqHost;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Inventory"] = _postgresConnectionString,
                ["RabbitMq:Host"] = _rabbitMqHost,
                ["RabbitMq:Username"] = "guest",
                ["RabbitMq:Password"] = "guest"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<InventoryDbContext>>();
            services.AddDbContext<InventoryDbContext>(options => options.UseNpgsql(_postgresConnectionString));
        });
    }
}
