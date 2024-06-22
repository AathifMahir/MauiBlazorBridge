using Microsoft.JSInterop;
using MauiBlazorBridge.Common;
using MauiBlazorBridge.Common.Exceptions;

namespace MauiBlazorBridge;
public sealed class Bridge : IBridge, IAsyncDisposable
{
    private readonly FrameworkIdentity _framework = GetFrameworkIdentity();
    public FrameworkIdentity Framework => _framework;

    private readonly Common.PlatformIdentity _platform = GetPlatform();
    public Common.PlatformIdentity Platform => _platform;
    public DeviceFormFactor FormFactor { get; set; } = DeviceFormFactor.Unknown;
    public EventHandler<DeviceFormFactor>? FormFactorChanged { get; set; }
    public bool IsListening => _isListening;

    string _platformVersion = GetPlatformVersion();
    public string PlatformVersion => _platformVersion;

    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    bool _isInitialized = false;
    bool _isListening = false;

    private readonly DotNetObjectReference<Bridge> _dotNetObjectReference;

    const string _blazorPath = "./_content/MauiBlazorBridge/blazorBridge.js";
    const string _mauiPath = "./blazorBridge.js";

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

        _isListening = isListenerEnabled;

        var module = await moduleTask.Value;

        if (Enum.TryParse<DeviceFormFactor>(await module.InvokeAsync<string>("getFormFactor"), out var deviceFormFactor))
        {
            FormFactor = deviceFormFactor;
        }

        if (FormFactorChanged is not null || isListenerEnabled)
        {
            await module.InvokeVoidAsync("registerResizeListener", _dotNetObjectReference);
        }

        _isInitialized = true;
    }

    public async Task InitializeListenerAsync()
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("registerResizeListener", _dotNetObjectReference);
        _isListening = true;
    }

    [JSInvokable]
    public ValueTask OnIdiomChangedCallback(string idiom)
    {
        if (Enum.TryParse<DeviceFormFactor>(idiom, out var deviceFormFactor))
        {
            OnIdiomChangedJsCallback(deviceFormFactor);
        }
        return ValueTask.CompletedTask;
    }

    private void OnIdiomChangedJsCallback(DeviceFormFactor deviceFormFactor)
    {
        if (!_isInitialized)
        {
            throw new MauiBlazorBridgeException("Bridge is not initialized.");
        };

        FormFactorChanged?.Invoke(this, deviceFormFactor);
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
            _isListening = false;
        }
    }

    private static Common.PlatformIdentity GetPlatform()
    {
#if ANDROID || IOS || WINDOWS || MACCATALYST
        if(DeviceInfo.Platform == DevicePlatform.Android)
            return Common.PlatformIdentity.Android;
        else if(DeviceInfo.Platform == DevicePlatform.iOS)
            return Common.PlatformIdentity.IOS;
        else if(DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            return Common.PlatformIdentity.Mac;
        else if(DeviceInfo.Platform == DevicePlatform.WinUI)
            return Common.PlatformIdentity.Windows;
        else
            return Common.PlatformIdentity.Unknown;
#else
        return Common.PlatformIdentity.Web;
#endif
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
