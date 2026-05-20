using FluentAssertions;
using IdentityService.Application.Commands;
using IdentityService.Domain;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Domain;

namespace IdentityService.Application.Tests;

public class SyncUserFromIdPCommandHandlerTests
{
    private static TestIdentityDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestIdentityDbContext(options);
    }

    [Fact]
    public async Task Handle_NewUser_ShouldCreateUserAndReturnId()
    {
        await using var dbContext = CreateDbContext();
        var handler = new SyncUserFromIdPCommandHandler(dbContext);
        var tenantId = new TenantId(Guid.NewGuid());

        var result = await handler.Handle(new SyncUserFromIdPCommand(
            tenantId, "ext-1", "user@example.com", "Test User", new List<string>(), new Dictionary<string, string>()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        dbContext.UserProfiles.Should().ContainSingle(u => u.ExternalId == "ext-1");
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldUpdateAndReturnSameId()
    {
        await using var dbContext = CreateDbContext();
        var tenantId = new TenantId(Guid.NewGuid());
        var existing = new UserProfile(tenantId, "ext-1", "old@example.com", "Old Name");
        dbContext.UserProfiles.Add(existing);
        await dbContext.SaveChangesAsync();
        var originalId = existing.Id;

        var handler = new SyncUserFromIdPCommandHandler(dbContext);
        var result = await handler.Handle(new SyncUserFromIdPCommand(
            tenantId, "ext-1", "new@example.com", "New Name", new List<string>(), new Dictionary<string, string>()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(originalId);
        var updated = await dbContext.UserProfiles.FirstAsync(u => u.ExternalId == "ext-1");
        updated.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task Handle_EmptyTenantId_ShouldReturnFailure()
    {
        await using var dbContext = CreateDbContext();
        var handler = new SyncUserFromIdPCommandHandler(dbContext);

        var result = await handler.Handle(new SyncUserFromIdPCommand(
            TenantId.Empty, "ext-1", "user@example.com", "Test User", new List<string>(), new Dictionary<string, string>()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
