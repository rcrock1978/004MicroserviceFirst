using SaaSCommon.Domain;

namespace CustomerService.Domain;

public sealed class Customer : Entity
{
    public string Email { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Phone { get; private set; }

    private Customer() { }

    public static Customer Create(string email, string name, string? phone, TenantId tenantId)
    {
        var customer = new Customer
        {
            Email = email,
            Name = name,
            Phone = phone,
            TenantId = tenantId
        };
        customer.AddDomainEvent(new CustomerCreated(customer.Id, customer.Email, customer.Name, customer.TenantId));
        return customer;
    }

    public void UpdateProfile(string name, string? phone)
    {
        Name = name;
        Phone = phone;
        SetUpdatedAt(DateTime.UtcNow);
        AddDomainEvent(new CustomerProfileUpdated(Id, Name, TenantId));
    }
}
