using FluentAssertions;
using TenantService.Application.Commands;
using TenantService.Application.Queries;
using TenantService.Domain;
using Microsoft.EntityFrameworkCore;

namespace TenantService.Application.Tests;

public class ActivateTenantCommandHandlerTests
{
    private static TestTenantDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestTenantDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestTenantDbContext(options);
    }

    [Fact]
    public async Task Handle_ExistingTenant_ShouldActivate()
    {
        await using var dbContext = CreateDbContext();
        var tenant = Tenant.Provision("Acme", "acme");
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();

        var handler = new ActivateTenantCommandHandler(dbContext);
        var result = await handler.Handle(new ActivateTenantCommand(tenant.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var updated = await dbContext.Tenants.FindAsync(tenant.Id);
        updated!.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public async Task Handle_NonExistingTenant_ShouldReturnNotFound()
    {
        await using var dbContext = CreateDbContext();
        var handler = new ActivateTenantCommandHandler(dbContext);

        var result = await handler.Handle(new ActivateTenantCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
