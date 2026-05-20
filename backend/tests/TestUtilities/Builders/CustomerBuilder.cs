using CustomerService.Domain;
using SaaSCommon.Domain;

namespace TestUtilities.Builders;

public class CustomerBuilder
{
    private string _email = "test@example.com";
    private string _name = "Test Customer";
    private string? _phone = "555-1234";
    private TenantId _tenantId = new TenantId(Guid.NewGuid());

    public CustomerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public CustomerBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CustomerBuilder WithPhone(string? phone)
    {
        _phone = phone;
        return this;
    }

    public CustomerBuilder WithTenantId(TenantId tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public Customer Build()
    {
        return Customer.Create(_email, _name, _phone, _tenantId);
    }
}
