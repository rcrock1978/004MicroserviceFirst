using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace SaaSCommon.Infrastructure.Resilience;

public static class ResilienceExtensions
{
    public static IServiceCollection AddResiliencePipelines(this IServiceCollection services)
    {
        var retryOptions = new RetryStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .HandleResult(r => !r.IsSuccessStatusCode),
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential
        };

        var circuitBreakerOptions = new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .HandleResult(r => !r.IsSuccessStatusCode),
            SamplingDuration = TimeSpan.FromSeconds(30),
            FailureRatio = 0.5,
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(30)
        };

        var timeoutOptions = new TimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        var resiliencePipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(retryOptions)
            .AddCircuitBreaker(circuitBreakerOptions)
            .AddTimeout(timeoutOptions)
            .Build();

        services.AddSingleton(resiliencePipeline);

        return services;
    }
}
