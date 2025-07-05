using AuthService.Application.Commands;
using FluentValidation.Results;

namespace AuthService.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TCommand> : IPipelineBehavior<TRequest, TCommand>
where TRequest : ICommand
where TCommand : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TCommand> Handle(TRequest request, RequestHandlerDelegate<TCommand> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(x => x.ValidateAsync(context, cancellationToken)));

            if (validationResults.SelectMany(x => x.Errors) 
                    is List<ValidationFailure> failures && failures.Any())
            {
                throw new ValidationException(failures);
            }
        }
        
        return await next(cancellationToken);
    }
}