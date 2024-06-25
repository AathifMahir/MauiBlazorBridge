using Microsoft.JSInterop;
using MauiBlazorBridge.Common;
using MauiBlazorBridge.Common.Exceptions;

namespace MauiBlazorBridge;
public sealed class Bridge : IBridge, IAsyncDisposable
{
    public FrameworkIdentity Framework { get; set; } = GetFrameworkIdentity();
    public PlatformIdentity Platform { get; set; } = PlatformIdentity.Unknown;
    public DeviceFormFactor FormFactor { get; set; } = DeviceFormFactor.Unknown;
    public EventHandler<DeviceFormFactor>? FormFactorChanged { get; set; }
    public bool IsListening { get; set; }
    public string PlatformVersion { get; set; } = GetPlatformVersion();

    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    bool _isInitialized = false;

    private readonly DotNetObjectReference<Bridge> _dotNetObjectReference;

#pragma warning disable IDE0051 // Remove unused private members
    const string _blazorPath = "./_content/MauiBlazorBridge/MauiBlazorBridge.js";
    const string _mauiPath = "./MauiBlazorBridge.js";
#pragma warning restore IDE0051 // Remove unused private members

    public Bridge(IJSRuntime jsRuntime)
    {
#if ANDROID || IOS || WINDOWS || MACCATALYST
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", _mauiPath).AsTask());
#else
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", _blazorPath).AsTask());
#endif

        _dotNetObjectReference = DotNetObjectReference.Create(this);
    }

    public async Task InitializeAsync(bool isListenerEnabled = false)
    {
        if (_isInitialized) return;

        IsListening = isListenerEnabled;

        var module = await moduleTask.Value ?? throw new MauiBlazorBridgeException("Failed to import the MauiBlazorHybrid.js");

        FormFactor = await GetFormFactorAsync(module);
        Platform = await GetPlatformAsync(module);

        if (isListenerEnabled)
            await module.InvokeVoidAsync("registerResizeListener", _dotNetObjectReference);

        _isInitialized = true;
    }

    public async Task InitializeListenerAsync()
    {
        var module = await moduleTask.Value ?? throw new MauiBlazorBridgeException("Failed to import the MauiBlazorHybrid.js");
        await module.InvokeVoidAsync("registerResizeListener", _dotNetObjectReference);
        IsListening = true;
    }

    [JSInvokable]
    public ValueTask OnIdiomChangedCallback(string idiom)
    {
        if (!_isInitialized)
            throw new MauiBlazorBridgeException("Bridge is not initialized.");

        if (Enum.TryParse<DeviceFormFactor>(idiom, out var deviceFormFactor))
            FormFactorChanged?.Invoke(this, deviceFormFactor);

        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("disposeListeners");
            _dotNetObjectReference.Dispose();
            await module.DisposeAsync();
        }
    }

    public async ValueTask DisposeListener()
    {
        if(moduleTask.IsValueCreated && IsListening)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("disposeListeners");
            IsListening = false;
        }
    }

#pragma warning disable CS1998
    private static async ValueTask<PlatformIdentity> GetPlatformAsync(IJSObjectReference module)
    {
#if ANDROID || IOS || WINDOWS || MACCATALYST
        if(DeviceInfo.Platform == DevicePlatform.Android)
            return PlatformIdentity.Android;
        else if(DeviceInfo.Platform == DevicePlatform.iOS)
            return PlatformIdentity.IOS;
        else if(DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            return PlatformIdentity.Mac;
        else if(DeviceInfo.Platform == DevicePlatform.WinUI)
            return PlatformIdentity.Windows;
        else
            return PlatformIdentity.Unknown;
#else
        if (Enum.TryParse<PlatformIdentity>(await module.InvokeAsync<string>("getPlatform"), out var platformIdentity))
            return platformIdentity;

        return PlatformIdentity.Unknown;
#endif
    }
#pragma warning restore CS1998

    private static async ValueTask<DeviceFormFactor> GetFormFactorAsync(IJSObjectReference module)
    {
        if (Enum.TryParse<DeviceFormFactor>(await module.InvokeAsync<string>("getFormFactor"), out var deviceFormFactor))
            return deviceFormFactor;

        return DeviceFormFactor.Unknown;
    }

    private static FrameworkIdentity GetFrameworkIdentity()
    {
#if ANDROID || IOS || WINDOWS || MACCATALYST
        return FrameworkIdentity.Maui;
#else
        return FrameworkIdentity.Blazor;
#endif
    }

    private static string GetPlatformVersion()
    {
#if ANDROID || IOS || WINDOWS || MACCATALYST
        return DeviceInfo.Version.ToString();
#else
        return "Unknown";
#endif
    }
}
