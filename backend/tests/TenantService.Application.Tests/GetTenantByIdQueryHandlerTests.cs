using FluentAssertions;
using TenantService.Application.Queries;
using TenantService.Domain;

namespace TenantService.Application.Tests;

public class GetTenantByIdQueryHandlerTests
{
    private static TestTenantDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestTenantDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestTenantDbContext(options);
    }

    [Fact]
    public async Task Handle_ExistingTenant_ShouldReturnTenant()
    {
        await using var dbContext = CreateDbContext();
        var tenant = Tenant.Provision("Acme", "acme");
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();

        var handler = new GetTenantByIdQueryHandler(dbContext);
        var result = await handler.Handle(new GetTenantByIdQuery(tenant.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Acme");
    }

    [Fact]
    public async Task Handle_NonExistingTenant_ShouldReturnNotFound()
    {
        await using var dbContext = CreateDbContext();
        var handler = new GetTenantByIdQueryHandler(dbContext);

        var result = await handler.Handle(new GetTenantByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
