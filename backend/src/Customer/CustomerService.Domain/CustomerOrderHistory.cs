using SaaSCommon.Domain;

namespace CustomerService.Domain;

public sealed class CustomerOrderHistory : Entity
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }

    public void SetTenantId(TenantId tenantId) => TenantId = tenantId;
}
