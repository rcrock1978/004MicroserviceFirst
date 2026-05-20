using FluentAssertions;
using CustomerService.Application.Queries;
using CustomerService.Domain;
using SaaSCommon.Domain;

namespace CustomerService.Application.Tests;

public class GetCustomerByIdQueryHandlerTests
{
    private static TestCustomerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestCustomerDbContext(options);
    }

    [Fact]
    public async Task Handle_ExistingCustomer_ShouldReturnCustomer()
    {
        await using var dbContext = CreateDbContext();
        var customer = Customer.Create("test@example.com", "Test Customer", null, new TenantId(Guid.NewGuid()));
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        var handler = new GetCustomerByIdQueryHandler(dbContext);
        var result = await handler.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test Customer");
    }

    [Fact]
    public async Task Handle_NonExistingCustomer_ShouldReturnNotFound()
    {
        await using var dbContext = CreateDbContext();
        var handler = new GetCustomerByIdQueryHandler(dbContext);

        var result = await handler.Handle(new GetCustomerByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
