using Microsoft.EntityFrameworkCore;
using IdentityService.Domain;

namespace IdentityService.Application;

public interface IIdentityDbContext
{
    DbSet<UserProfile> UserProfiles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
