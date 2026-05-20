using PaymentService.Application.Ports;

namespace PaymentService.Infrastructure.Providers;

public sealed class SimulatedPaymentProvider : IPaymentProvider
{
    private readonly decimal? _failBelowAmount;
    private readonly int? _failureRatePercent;
    private readonly TimeSpan? _latency;
    private readonly Random _random = new();

    public SimulatedPaymentProvider(decimal? failBelowAmount = null, int? failureRatePercent = null, TimeSpan? latency = null)
    {
        _failBelowAmount = failBelowAmount;
        _failureRatePercent = failureRatePercent;
        _latency = latency;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (_latency.HasValue)
        {
            await Task.Delay(_latency.Value, cancellationToken);
        }

        if (_failureRatePercent.HasValue && _random.Next(100) < _failureRatePercent.Value)
        {
            return new PaymentResult(false, null, "Simulated random payment failure.");
        }

        var threshold = _failBelowAmount ?? 0;
        if (request.Amount <= threshold)
        {
            return new PaymentResult(false, null, $"Amount {request.Amount} is less than or equal to threshold {threshold}.");
        }

        var providerReference = $"sim_{request.OrderId:N}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        return new PaymentResult(true, providerReference);
    }

    public async Task<PaymentResult> RefundPaymentAsync(string providerReference, CancellationToken cancellationToken = default)
    {
        if (_latency.HasValue)
        {
            await Task.Delay(_latency.Value, cancellationToken);
        }

        return new PaymentResult(true, providerReference + "_refund");
    }
}
