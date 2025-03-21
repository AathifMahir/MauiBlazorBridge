
using MauiBlazorBridge.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MauiBlazorBridge;
public static class BuilderExtension
{
    /// <summary>
    /// this is used to register the bridge for the blazor app and razor class libraries
    /// </summary>
    public static IServiceCollection AddMauiBlazorBridge(this IServiceCollection services)
    {

#if ANDROID || IOS || WINDOWS || MACCATALYST
        services.AddScoped<IBridge, Services.Bridge>();
        services.AddScoped<IBridgeConnectivity, Services.BridgeConnectivity>();
        services.AddScoped<IBridgeFormFactor, Services.BridgeFormFactor>();

#else

        services.AddScoped<IBridge, Services.BridgeWeb>();
        services.AddScoped<IBridgeConnectivity, Services.BridgeConnectivityWeb>();
        services.AddScoped<IBridgeFormFactor, Services.BridgeFormFactorWeb>();

#endif

        return services;
    }
}
