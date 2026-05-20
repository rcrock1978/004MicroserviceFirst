using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace CustomerService.IntegrationTests;

public class IntegrationTestFixture : IAsyncLifetime
{
    public PostgreSqlContainer Postgres { get; } = new PostgreSqlBuilder().Build();
    public RabbitMqContainer RabbitMq { get; } = new RabbitMqBuilder().Build();

    public string PostgresConnectionString => Postgres.GetConnectionString();
    public string RabbitMqHost => $"rabbitmq://guest:guest@{RabbitMq.Hostname}:{RabbitMq.GetMappedPublicPort(5672)}";

    public async Task InitializeAsync()
    {
        await Postgres.StartAsync();
        await RabbitMq.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await RabbitMq.DisposeAsync();
        await Postgres.DisposeAsync();
    }
}
