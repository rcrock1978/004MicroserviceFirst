using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CustomerService.Infrastructure;

namespace CustomerService.IntegrationTests;

public class CustomerServiceTestFactory : WebApplicationFactory<Program>
{
    private readonly string _postgresConnectionString;
    private readonly string _rabbitMqHost;

    public CustomerServiceTestFactory(string postgresConnectionString, string rabbitMqHost)
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
                ["ConnectionStrings:Customer"] = _postgresConnectionString,
                ["RabbitMq:Host"] = _rabbitMqHost,
                ["RabbitMq:Username"] = "guest",
                ["RabbitMq:Password"] = "guest"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CustomerDbContext>>();
            services.AddDbContext<CustomerDbContext>(options => options.UseNpgsql(_postgresConnectionString));
        });
    }
}
