using MediatR;

namespace AuthService.Application.Queries.QueryHandlers;

public interface IQueryHandler<in TQuery, TResponse> : 
    IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse> 
    where TResponse : notnull
{
}