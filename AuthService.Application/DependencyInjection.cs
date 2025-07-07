using System.Reflection;
using AuthService.Application.Common.Behaviors;
using AuthService.Application.DomainEvents.DomainEventsHandlers.Auth;
using AuthService.Application.DomainEvents.DomainEventsHandlers.Users;
using AuthService.Application.Interfaces;
using AuthService.Application.Services.DomainEvents;
using AuthService.Domain.DomainEvents.Auth;
using AuthService.Domain.DomainEvents.Users;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        
        services.AddScoped<IDomainDispatcher, DomainDispatcher>();
        
        BindValidators(services);
        BindDomainEventHandlers(services);

        return services;
    }

    private static void BindValidators(IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }

    private static void BindDomainEventHandlers(IServiceCollection services)
    {
        services.AddScoped<IDomainEventHandler<UserLoggedInDomainEvent>, UserLoggedInDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<UserLoggedOutDomainEvent>, UserLoggedOutDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<UserSignedUpDomainEvent>, UserSignedUpDomainEventHandler>();
        
        services.AddScoped<IDomainEventHandler<UserCreatedDomainEvent>, UserCreatedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<UserDeletedDomainEvent>, UserDeletedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<UserUpdatedDomainEvent>, UserUpdatedDomainEventHandler>();
    }
}