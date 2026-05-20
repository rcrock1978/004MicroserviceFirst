using FluentAssertions;
using IdentityService.Application.Dtos;
using IdentityService.Application.Queries;
using IdentityService.Domain;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Domain;

namespace IdentityService.Application.Tests;

public class GetUserProfileByIdQueryHandlerTests
{
    private static TestIdentityDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestIdentityDbContext(options);
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldReturnDto()
    {
        await using var dbContext = CreateDbContext();
        var tenantId = new TenantId(Guid.NewGuid());
        var user = new UserProfile(tenantId, "ext-1", "user@example.com", "Test User");
        dbContext.UserProfiles.Add(user);
        await dbContext.SaveChangesAsync();

        var handler = new GetUserProfileByIdQueryHandler(dbContext);
        var result = await handler.Handle(new GetUserProfileByIdQuery(user.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<UserProfileDto>();
        result.Value.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task Handle_NonExistingUser_ShouldReturnNotFound()
    {
        await using var dbContext = CreateDbContext();
        var handler = new GetUserProfileByIdQueryHandler(dbContext);

        var result = await handler.Handle(new GetUserProfileByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
