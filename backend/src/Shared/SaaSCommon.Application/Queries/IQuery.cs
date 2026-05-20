using MediatR;

namespace SaaSCommon.Application.Queries;

public interface IQuery<TResponse> : IRequest<TResponse>
{
}
