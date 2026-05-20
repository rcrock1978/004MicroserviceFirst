using FluentAssertions;
using CustomerService.Application.Commands;
using CustomerService.Domain;
using SaaSCommon.Domain;
using SaaSCommon.Infrastructure.Tenancy;
using NSubstitute;

namespace CustomerService.Application.Tests;

public class CreateCustomerCommandHandlerTests
{
    private static TestCustomerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestCustomerDbContext(options);
    }

    [Fact]
    public async Task Handle_NewCustomer_ShouldCreateAndReturnId()
    {
        await using var dbContext = CreateDbContext();
        var tenantService = Substitute.For<ICurrentTenantService>();
        tenantService.TenantId.Returns(new TenantId(Guid.NewGuid()));
        var publisher = Substitute.For<MassTransit.IPublishEndpoint>();

        var handler = new CreateCustomerCommandHandler(dbContext, tenantService, publisher);
        var result = await handler.Handle(new CreateCustomerCommand("test@example.com", "Test Customer", "555-1234"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        dbContext.Customers.Should().ContainSingle(c => c.Email == "test@example.com");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldReturnConflict()
    {
        await using var dbContext = CreateDbContext();
        var tenantId = new TenantId(Guid.NewGuid());
        dbContext.Customers.Add(Customer.Create("test@example.com", "Existing", null, tenantId));
        await dbContext.SaveChangesAsync();

        var tenantService = Substitute.For<ICurrentTenantService>();
        tenantService.TenantId.Returns(tenantId);
        var publisher = Substitute.For<MassTransit.IPublishEndpoint>();

        var handler = new CreateCustomerCommandHandler(dbContext, tenantService, publisher);
        var result = await handler.Handle(new CreateCustomerCommand("test@example.com", "Test Customer", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Conflict");
    }
}
