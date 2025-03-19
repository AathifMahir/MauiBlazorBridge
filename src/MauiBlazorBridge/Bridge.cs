#pragma warning disable CS1998
#pragma warning disable IDE0051
using Microsoft.JSInterop;
using System.Text.Json;

namespace MauiBlazorBridge;
public sealed class Bridge : IBridge, IAsyncDisposable
{
    public Framework Framework { get; set; } = GetFrameworkIdentity();
    public PlatformIdentity Platform { get; set; } = PlatformIdentity.Unknown;
    public DeviceFormFactor DeviceFormFactor { get; set; } = DeviceFormFactor.UnknownState();
    public bool InternetConnection { get; set; } = false;
    public EventHandler<DeviceFormFactor>? FormFactorChanged { get; set; }
    public EventHandler<PlatformIdentity>? PlatformChanged { get; set; }
    public event EventHandler<bool>? InternetConnectionChanged;

    public string PlatformVersion { get; set; } = GetPlatformVersion();
    public ListenerType ListenerType { get; set; }



    private readonly Lazy<Task<IJSObjectReference>> moduleTask;
    private readonly IJSRuntime _jsRuntime;

    bool _isInitialized = false;
    int _listenerCount = 0;
    int _internetConnectionIntervalinSeconds = 5;
    CancellationTokenSource _cts = new();

    private readonly DotNetObjectReference<Bridge> _dotNetObjectReference;

    const string _releasePath = "./_content/AathifMahir.MauiBlazor.MauiBlazorBridge/MauiBlazorBridge.js";
    const string _debugPath = "./MauiBlazorBridge.js";

    public Bridge(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;

#if DEBUG && ANDROID || DEBUG && IOS || DEBUG && WINDOWS || DEBUG && MACCATALYST
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", _debugPath).AsTask());
#else
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", _releasePath).AsTask());
#endif

        _dotNetObjectReference = DotNetObjectReference.Create(this);

    }

    public async Task InitializeAsync(ListenerType listenerType = ListenerType.None, int? internetConnectionIntervalinSeconds = null)
    {
        if (_isInitialized) return;

        ListenerType = listenerType;
        _internetConnectionIntervalinSeconds = internetConnectionIntervalinSeconds ?? 5;

        var module = await moduleTask.Value ?? throw new MauiBlazorBridgeException("Failed to import the MauiBlazorHybrid.js");

        DeviceFormFactor = await GetFormFactorAsync(module);
        Platform = await GetPlatformAsync(module);
        PlatformChanged?.Invoke(this, Platform);
        var isOnline = await GetInternetConnectionStatus(module);
        InternetConnection = isOnline;

        _isInitialized = true;

        if (listenerType is ListenerType.Global)
            await InitializeListenerAsync(module);

        await InitializeNetworkListenerAsync(module);
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

    public async Task InitializeNetworkListenerAsync(IJSObjectReference? jsObject = null)
    {
        if (!_isInitialized)
            throw new MauiBlazorBridgeException("Bridge is not initialized. Make sure to add BridgeProvider Component");

#if ANDROID || IOS || WINDOWS || MACCATALYST
        Connectivity.ConnectivityChanged += NetworkConnectivityChanged;
#else

        var module = jsObject ?? await moduleTask.Value ?? throw new MauiBlazorBridgeException("Failed to import the MauiBlazorHybrid.js");
        await module.InvokeVoidAsync("registerNetworkListener", _dotNetObjectReference, _internetConnectionIntervalinSeconds);
#endif
    }

#if ANDROID || IOS || WINDOWS || MACCATALYST
    private void NetworkConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        InternetConnection = e.NetworkAccess == NetworkAccess.Internet;
        InternetConnectionChanged?.Invoke(this, InternetConnection);
    }
#endif

    [JSInvokable]
    public ValueTask OnIdiomChangedCallback(string idiomString)
    {
        if (!_isInitialized)
            throw new MauiBlazorBridgeException("Bridge is not initialized.");

        var idiom = JsonSerializer.Deserialize<DeviceFormFactor>(idiomString);

        if (idiom is not null)
        {
            DeviceFormFactor = idiom;
            FormFactorChanged?.Invoke(this, idiom);
        }
        return ValueTask.CompletedTask;
    }

    [JSInvokable]
    public void NotifyNetworkStatusChanged(bool onlineStatus)
    {
        InternetConnectionChanged?.Invoke(this, onlineStatus);
        InternetConnection = onlineStatus;
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

    public async ValueTask DisposeNetworkListenerAsync()
    {
#if ANDROID || IOS || WINDOWS || MACCATALYST
        Connectivity.ConnectivityChanged -= NetworkConnectivityChanged;
#else
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("disposeNetworkListener");
        }
#endif
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
            return new DeviceFormFactor(FormFactor.Mobile, 0, 0);
        else if(DeviceInfo.Idiom == DeviceIdiom.Tablet)
            return new DeviceFormFactor(FormFactor.Tablet, 0, 0);
        else if(DeviceInfo.Idiom == DeviceIdiom.Desktop)
            return new DeviceFormFactor(FormFactor.Desktop, 0, 0);
        else
            return DeviceFormFactor.UnknownState();
#else

        var formFactorString = await module.InvokeAsync<string>("getFormFactor");

        if(string.IsNullOrEmpty(formFactorString))
            return DeviceFormFactor.UnknownState();

        var formFactor = JsonSerializer.Deserialize<DeviceFormFactor>(formFactorString);
        return formFactor ?? DeviceFormFactor.UnknownState();
#endif
    }

    private static Framework GetFrameworkIdentity()
    {
#if ANDROID || IOS || WINDOWS || MACCATALYST
        return Framework.Maui;
#else
        return Framework.Blazor;
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

    private async static ValueTask<bool> GetInternetConnectionStatus(IJSObjectReference module)
    {
#if ANDROID || IOS || WINDOWS || MACCATALYST
        return Connectivity.NetworkAccess == NetworkAccess.Internet;
#else
        var isOnline = await module.InvokeAsync<bool>("getNetworkStatus");
        return isOnline;
#endif
    }

#if ANDROID || IOS || WINDOWS || MACCATALYST
    private void WindowSizeChanged(object? sender, EventArgs args)
    {
        if (Application.Current is null || Application.Current.MainPage is null) return;

        var width = Application.Current.MainPage.Window.Width;
        var height = Application.Current.MainPage.Window.Height;

        DeviceFormFactor newFormFactor;

        if (width <= 767)
            newFormFactor = new DeviceFormFactor(FormFactor.Mobile, width, height);
        else if(width >= 768 && width <= 1023)
            newFormFactor = new DeviceFormFactor(FormFactor.Tablet, width, height);
        else if(width >= 1024)
            newFormFactor = new DeviceFormFactor(FormFactor.Desktop, width, height);
        else
            newFormFactor = DeviceFormFactor.UnknownState(width, height);

        if (newFormFactor.FormFactor != DeviceFormFactor.FormFactor)
        {
            DeviceFormFactor = newFormFactor;
            FormFactorChanged?.Invoke(this, newFormFactor);
        }
    }
#endif

}
#pragma warning restore CS1998
#pragma warning restore IDE0051