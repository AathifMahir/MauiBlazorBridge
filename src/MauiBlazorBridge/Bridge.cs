using Microsoft.JSInterop;
using MauiBlazorBridge.Common;
using MauiBlazorBridge.Common.Exceptions;

namespace MauiBlazorBridge;
public sealed class Bridge : IBridge, IAsyncDisposable
{
    private readonly FrameworkIdentity _framework = GetFrameworkIdentity();
    public FrameworkIdentity Framework => _framework;

    private readonly Common.Platform _platform = GetPlatform();
    public Common.Platform Platform => _platform;
    public DeviceFormFactor Idiom { get; set; } = DeviceFormFactor.Unknown;
    public EventHandler<DeviceFormFactor>? IdiomChanged { get; set; }
    public bool IsListening => _isListening;

    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    static WeakReference<Bridge> _currentBridge = default!;

    bool _isInitialized = false;
    bool _isListening = false;

    public Bridge(IJSRuntime jsRuntime)
    {
        if (_currentBridge != null)
        {
            throw new Exception("Only one instance of Bridge is allowed.");
        }
        _currentBridge = new WeakReference<Bridge>(this);

        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/MauiBlazorBridge/blazorBridge.js").AsTask());

    }

    public async Task InitializeAsync(bool isListenerEnabled = false)
    {
        if (_isInitialized) return;

        _isListening = isListenerEnabled;

        var module = await moduleTask.Value;

        if (Enum.TryParse<DeviceFormFactor>(await module.InvokeAsync<string>("getFormFactor"), out var deviceFormFactor))
        {
            Idiom = deviceFormFactor;
        }

        if (IdiomChanged is not null || isListenerEnabled)
        {
            await module.InvokeVoidAsync("registerResizeListener", "MauiBlazorBridge");
        }

        _isInitialized = true;
    }

    [JSInvokable]
    public static void OnIdiomChangedCallback(string idiom)
    {
        if (Enum.TryParse<DeviceFormFactor>(idiom, out var deviceFormFactor))
        {
            if (_currentBridge.TryGetTarget(out var bridge))
            {
                bridge.OnIdiomChangedJsCallback(deviceFormFactor);
            }
        }
    }

    private void OnIdiomChangedJsCallback(DeviceFormFactor deviceFormFactor)
    {
        if (!_isInitialized)
        {
            throw new MauiBlazorBridgeException("Bridge is not initialized.");
        };

        IdiomChanged?.Invoke(this, deviceFormFactor);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("disposeListeners");
            await module.DisposeAsync();
        }
    }

    private static Common.Platform GetPlatform()
    {
#if ANDROID || IOS || WINDOWS || MACCATALYST
        if(DeviceInfo.Platform == DevicePlatform.Android)
            return Common.Platform.Android;
        else if(DeviceInfo.Platform == DevicePlatform.iOS)
            return Common.Platform.iOS;
        else if(DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            return Common.Platform.Mac;
        else
            return Common.Platform.Unknown;
#else
        return Common.Platform.Web;
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
}
