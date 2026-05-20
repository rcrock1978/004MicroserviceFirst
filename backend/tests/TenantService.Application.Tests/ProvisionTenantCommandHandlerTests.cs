using FluentAssertions;
using TenantService.Application.Commands;
using TenantService.Domain;
using Microsoft.EntityFrameworkCore;

namespace TenantService.Application.Tests;

public class ProvisionTenantCommandHandlerTests
{
    private static TestTenantDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestTenantDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestTenantDbContext(options);
    }

    [Fact]
    public async Task Handle_NewTenant_ShouldCreateTenant()
    {
        await using var dbContext = CreateDbContext();
        var handler = new ProvisionTenantCommandHandler(dbContext);

        var result = await handler.Handle(new ProvisionTenantCommand("Acme", "acme"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        dbContext.Tenants.Should().ContainSingle(t => t.Slug == "acme");
    }

    [Fact]
    public async Task Handle_DuplicateSlug_ShouldReturnConflict()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Tenants.Add(Tenant.Provision("Acme", "acme"));
        await dbContext.SaveChangesAsync();

        var handler = new ProvisionTenantCommandHandler(dbContext);
        var result = await handler.Handle(new ProvisionTenantCommand("Acme 2", "acme"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Conflict");
    }
}
