using MediatR;

namespace SaaSCommon.Application.Commands;

public interface ICommand : IRequest
{
}

public interface ICommand<TResponse> : IRequest<TResponse>
{
}
