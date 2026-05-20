using FluentAssertions;
using CustomerService.Domain;
using SaaSCommon.Domain;

namespace CustomerService.Domain.Tests;

public class CustomerTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var customer = Customer.Create("test@example.com", "Test Customer", "555-1234", tenantId);

        customer.Email.Should().Be("test@example.com");
        customer.Name.Should().Be("Test Customer");
        customer.Phone.Should().Be("555-1234");
        customer.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void Create_ShouldRaiseCustomerCreatedEvent()
    {
        var customer = Customer.Create("test@example.com", "Test Customer", null, new TenantId(Guid.NewGuid()));

        customer.DomainEvents.Should().ContainSingle(e => e is CustomerCreated);
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateNameAndPhone()
    {
        var customer = Customer.Create("test@example.com", "Old Name", "555-0000", new TenantId(Guid.NewGuid()));
        customer.ClearDomainEvents();

        customer.UpdateProfile("New Name", "555-9999");

        customer.Name.Should().Be("New Name");
        customer.Phone.Should().Be("555-9999");
        customer.DomainEvents.Should().ContainSingle(e => e is CustomerProfileUpdated);
    }
}
