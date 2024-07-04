#pragma warning disable CS1998
#pragma warning disable IDE0051
using Microsoft.JSInterop;
using System.Diagnostics;

namespace MauiBlazorBridge;
public sealed class Bridge : IBridge, IAsyncDisposable
{
    public FrameworkIdentity Framework { get; set; } = GetFrameworkIdentity();
    public PlatformIdentity Platform { get; set; } = PlatformIdentity.Unknown;
    public DeviceFormFactor FormFactor { get; set; } = DeviceFormFactor.Unknown;
    public EventHandler<DeviceFormFactor>? FormFactorChanged { get; set; }
    public EventHandler<PlatformIdentity>? PlatformChanged { get; set; }
    public string PlatformVersion { get; set; } = GetPlatformVersion();
    public ListenerType ListenerType { get; set; }

    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    bool _isInitialized = false;
    int _listenerCount = 0;
    CancellationTokenSource _cts = new();

    private readonly DotNetObjectReference<Bridge> _dotNetObjectReference;

    const string _releasePath = "./_content/AathifMahir.MauiBlazor.MauiBlazorBridge/MauiBlazorBridge.js";
    const string _debugPath = "./MauiBlazorBridge.js";

    public Bridge(IJSRuntime jsRuntime)
    {
#if DEBUG && ANDROID || DEBUG && IOS || DEBUG && WINDOWS || DEBUG && MACCATALYST
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", _debugPath).AsTask());
#else
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", _releasePath).AsTask());
#endif

        _dotNetObjectReference = DotNetObjectReference.Create(this);
    }

    public async Task InitializeAsync(ListenerType listenerType = ListenerType.None)
    {
        if (_isInitialized) return;

        ListenerType = listenerType;

        var module = await moduleTask.Value ?? throw new MauiBlazorBridgeException("Failed to import the MauiBlazorHybrid.js");

        FormFactor = await GetFormFactorAsync(module);
        Platform = await GetPlatformAsync(module);
        PlatformChanged?.Invoke(this, Platform);

        if (listenerType is ListenerType.Global)
            await InitializeListenerAsync(module);

        _isInitialized = true;
    }


    public async Task InitializeListenerAsync(IJSObjectReference? jsObject = null)
    {
        if (!_isInitialized)
            throw new MauiBlazorBridgeException("Bridge is not initialized. Make sure to add BridgeProvider Component");

        OnNewListener();

        if (_listenerCount > 0 || ListenerType is ListenerType.Suppressed)
        {
            _listenerCount++;
            return;
        }

#if ANDROID || IOS || WINDOWS || MACCATALYST
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Application.Current is null || Application.Current.MainPage is null) return;
            Application.Current.MainPage.Window.SizeChanged += WindowSizeChanged;
        });
#else
        var module = jsObject ?? await moduleTask.Value ?? throw new MauiBlazorBridgeException("Failed to import the MauiBlazorHybrid.js");
        await module.InvokeVoidAsync("registerResizeListener", _dotNetObjectReference);
#endif
        _listenerCount++;
    }

    [JSInvokable]
    public ValueTask OnIdiomChangedCallback(string idiom)
    {
        if (!_isInitialized)
            throw new MauiBlazorBridgeException("Bridge is not initialized.");

        if (Enum.TryParse<DeviceFormFactor>(idiom, out var deviceFormFactor))
        {
            FormFactor = deviceFormFactor;
            FormFactorChanged?.Invoke(this, deviceFormFactor);
        }
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;

            if (_listenerCount > 0 && ListenerType is not ListenerType.Suppressed)
            {
#if ANDROID || IOS || WINDOWS || MACCATALYST
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (Application.Current is null || Application.Current.MainPage is null) return;
                    Application.Current.MainPage.Window.SizeChanged -= WindowSizeChanged;
                });
#else
                await module.InvokeVoidAsync("disposeListeners");
#endif
            }
                

            _dotNetObjectReference.Dispose();
            await module.DisposeAsync();
        }
    }

    public async ValueTask DisposeListener()
    {

        try
        {
            if (ListenerType is ListenerType.Global) return;

            if (_listenerCount is 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), _cts.Token);
#if ANDROID || IOS || WINDOWS || MACCATALYST
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Application.Current is null || Application.Current.MainPage is null) return;
                Application.Current.MainPage.Window.SizeChanged -= WindowSizeChanged;
            });
#else
                if (moduleTask.IsValueCreated)
                {
                    var module = await moduleTask.Value;
                    await module.InvokeVoidAsync("disposeListeners");
                }
#endif
                _listenerCount = 0;
            }
            else if (_listenerCount > 0)
                _listenerCount--;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("DisposeListener Task was cancelled");
        }
    }

    private void OnNewListener()
    {
        _cts.Cancel();
        _cts = new();
    }
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
    private static async ValueTask<DeviceFormFactor> GetFormFactorAsync(IJSObjectReference module)
    {
#if ANDROID || IOS || WINDOWS || MACCATALYST
        if(DeviceInfo.Idiom == DeviceIdiom.Phone)
            return DeviceFormFactor.Mobile;
        else if(DeviceInfo.Idiom == DeviceIdiom.Tablet)
            return DeviceFormFactor.Tablet;
        else if(DeviceInfo.Idiom == DeviceIdiom.Desktop)
            return DeviceFormFactor.Desktop;
        else
            return DeviceFormFactor.Unknown;
#else

        if (Enum.TryParse<DeviceFormFactor>(await module.InvokeAsync<string>("getFormFactor"), out var deviceFormFactor))
            return deviceFormFactor;

        return DeviceFormFactor.Unknown;
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

#if ANDROID || IOS || WINDOWS || MACCATALYST
    private void WindowSizeChanged(object? sender, EventArgs args)
    {
        if (Application.Current is null || Application.Current.MainPage is null) return;

        var width = Application.Current.MainPage.Window.Width;

        DeviceFormFactor newFormFactor;

        if (width <= 767)
            newFormFactor = DeviceFormFactor.Mobile;
        else if(width >= 768 && width <= 1023)
            newFormFactor = DeviceFormFactor.Tablet;
        else if(width >= 1024)
            newFormFactor = DeviceFormFactor.Desktop;
        else
            newFormFactor = DeviceFormFactor.Unknown;

        if (newFormFactor != FormFactor)
        {
            FormFactor = newFormFactor;
            FormFactorChanged?.Invoke(this, newFormFactor);
        }
    }
#endif

}
#pragma warning restore CS1998
#pragma warning restore IDE0051