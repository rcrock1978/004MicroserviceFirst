using MediatR;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using CustomerService.Domain;
using SaaSCommon.Infrastructure.Tenancy;

namespace CustomerService.Application.Commands;

public sealed record UpdateCustomerProfileCommand(Guid CustomerId, string Name, string? Phone) : ICommand<Result<object>>;

public sealed class UpdateCustomerProfileCommandHandler(
    ICustomerDbContext dbContext,
    MassTransit.IPublishEndpoint publishEndpoint)
    : IRequestHandler<UpdateCustomerProfileCommand, Result<object>>
{
    public async Task<Result<object>> Handle(UpdateCustomerProfileCommand request, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
        if (customer is null)
        {
            return Result<object>.Failure(Error.NotFoundWithDetails($"Customer '{request.CustomerId}' not found."));
        }

        customer.UpdateProfile(request.Name, request.Phone);

        await publishEndpoint.Publish(
            new Contracts.CustomerProfileUpdatedEvent(customer.Id, customer.Name, customer.TenantId, DateTime.UtcNow),
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
