using Testcontainers.RabbitMq;

namespace TestUtilities;

public class RabbitMqContainerFixture : IAsyncLifetime
{
    public RabbitMqContainer Container { get; } = new RabbitMqBuilder()
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    public string Host => $"rabbitmq://guest:guest@{Container.Hostname}:{Container.GetMappedPublicPort(5672)}";

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}
