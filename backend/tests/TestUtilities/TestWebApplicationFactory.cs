using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TestUtilities;

public abstract class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly string _connectionString;
    private readonly string _rabbitMqHost;

    protected TestWebApplicationFactory(string connectionString, string rabbitMqHost)
    {
        _connectionString = connectionString;
        _rabbitMqHost = rabbitMqHost;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions));
            ConfigureDatabase(services, _connectionString);
            ConfigureMessageBroker(services, _rabbitMqHost);
        });
    }

    protected abstract void ConfigureDatabase(IServiceCollection services, string connectionString);
    protected abstract void ConfigureMessageBroker(IServiceCollection services, string rabbitMqHost);
}
