namespace TestUtilities;

[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<PostgresContainerFixture>, ICollectionFixture<RabbitMqContainerFixture>
{
}
