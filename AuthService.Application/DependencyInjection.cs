using System.Reflection;
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
        
        services.AddScoped<IDomainEventHandler<UserLoggedInDomainEvent>, UserLoggedInDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<UserLoggedOutDomainEvent>, UserLoggedOutDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<UserSignedUpDomainEvent>, UserSignedUpDomainEventHandler>();
        
        services.AddScoped<IDomainEventHandler<UserCreatedDomainEvent>, UserCreatedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<UserDeletedDomainEvent>, UserDeletedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<UserUpdatedDomainEvent>, UserUpdatedDomainEventHandler>();
        
        return services;
    }
}