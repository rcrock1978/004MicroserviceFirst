namespace PaymentService.Application.Ports;

public sealed record PaymentRequest(Guid OrderId, decimal Amount, Guid TenantId);
public sealed record PaymentResult(bool Success, string? ProviderReference = null, string? ErrorMessage = null);

public interface IPaymentProvider
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResult> RefundPaymentAsync(string providerReference, CancellationToken cancellationToken = default);
}
