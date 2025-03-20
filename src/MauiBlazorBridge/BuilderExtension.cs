
using Microsoft.Extensions.DependencyInjection;

namespace MauiBlazorBridge;
public static class BuilderExtension
{
    /// <summary>
    /// this is used to register the bridge for the blazor app and razor class libraries
    /// </summary>
    public static IServiceCollection AddMauiBlazorBridge(this IServiceCollection services)
    {
        services.AddScoped<IBridge, Bridge>();

#if ANDROID || IOS || WINDOWS || MACCATALYST

        services.AddScoped<IBridgeConnectivity, Services.BridgeConnectivity>();

#else

        services.AddScoped<IBridgeConnectivity, Services.BridgeConnectivityWeb>();

#endif

        return services;
    }
}
