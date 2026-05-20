using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderService.Infrastructure;

namespace OrderService.IntegrationTests;

public class OrderServiceTestFactory : WebApplicationFactory<Program>
{
    private readonly string _postgresConnectionString;
    private readonly string _rabbitMqHost;

    public OrderServiceTestFactory(string postgresConnectionString, string rabbitMqHost)
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
                ["ConnectionStrings:Order"] = _postgresConnectionString,
                ["RabbitMq:Host"] = _rabbitMqHost,
                ["RabbitMq:Username"] = "guest",
                ["RabbitMq:Password"] = "guest"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<OrderDbContext>>();
            services.AddDbContext<OrderDbContext>(options => options.UseNpgsql(_postgresConnectionString));
        });
    }
}
