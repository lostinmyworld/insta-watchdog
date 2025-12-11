using InstaWatchdogApp.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Social.Oversharers.Extensions;
using Social.Overthinkers.Extensions;

namespace InstaWatchdogApp.Extensions;

public static class BootstrapperExtensions
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSocialOverThinkers();
        services.AddSocialOverSharers();

        services.AddSingleton<IParser, Parser>();

        return services;
    }
}
