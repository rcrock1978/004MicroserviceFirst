using MediatR;
using Microsoft.EntityFrameworkCore;
using SaaSCommon.Application.Commands;
using SaaSCommon.Domain;
using CustomerService.Domain;
using SaaSCommon.Infrastructure.Tenancy;

namespace CustomerService.Application.Commands;

public sealed record CreateCustomerCommand(string Email, string Name, string? Phone) : ICommand<Result<Guid>>;

public sealed class CreateCustomerCommandHandler(
    ICustomerDbContext dbContext,
    ICurrentTenantService currentTenantService,
    MassTransit.IPublishEndpoint publishEndpoint)
    : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Email == request.Email, cancellationToken);

        if (existing is not null)
        {
            return Result<Guid>.Failure(Error.Conflict with { Details = $"Customer with email '{request.Email}' already exists." });
        }

        var customer = Customer.Create(request.Email, request.Name, request.Phone, currentTenantService.TenantId);
        dbContext.Customers.Add(customer);

        await publishEndpoint.Publish(
            new Contracts.CustomerCreatedEvent(customer.Id, customer.Email, customer.Name, customer.TenantId, DateTime.UtcNow),
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(customer.Id);
    }
}
