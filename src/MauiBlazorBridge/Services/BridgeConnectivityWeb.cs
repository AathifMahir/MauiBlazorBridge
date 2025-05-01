using Microsoft.JSInterop;

namespace MauiBlazorBridge.Services;
public sealed class BridgeConnectivityWeb : IBridgeConnectivity, IAsyncDisposable
{
    readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    readonly DotNetObjectReference<BridgeConnectivityWeb> _dotNetObjectReference;

    const string _releasePath = "./_content/AathifMahir.MauiBlazor.MauiBlazorBridge/BridgeConnectivity.js";
    const string _debugPath = "./BridgeConnectivity.js";

    bool _isInitialized = false;

    public event EventHandler<bool>? InternetConnectionChanged;

    public BridgeConnectivityWeb(IJSRuntime jSRuntime)
    {
        _moduleTask = new(() => jSRuntime.InvokeAsync<IJSObjectReference>(
            "import",
#if DEBUG && ANDROID || DEBUG && IOS || DEBUG && WINDOWS || DEBUG && MACCATALYST
            _debugPath
#else
            _releasePath
#endif
            ).AsTask());

        _dotNetObjectReference = DotNetObjectReference.Create(this);
    }


    public bool IsInternetConnected { get; private set; } = true;

    public async Task InitializeAsync(int internetConnectionInvervalInSeconds = 10)
    {
        if (_isInitialized) return;

        var module = await _moduleTask.Value ?? throw new MauiBlazorBridgeException("Failed to import the BridgeConnectivity.js");

        IsInternetConnected = await module.InvokeAsync<bool>("getNetworkStatus");
        InternetConnectionChanged?.Invoke(this, IsInternetConnected);

        await module.InvokeVoidAsync("initializeListener", _dotNetObjectReference, internetConnectionInvervalInSeconds);

        _isInitialized = true;
    }


    [JSInvokable]
    public void NotifyConnectivityStatusChanged(bool onlineStatus)
    {
        if(IsInternetConnected != onlineStatus)
        {
            IsInternetConnected = onlineStatus;
            InternetConnectionChanged?.Invoke(this, onlineStatus);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_isInitialized || !_moduleTask.IsValueCreated) return;

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("disposeListener", _dotNetObjectReference);
        await module.DisposeAsync();

        _dotNetObjectReference.Dispose();
        _isInitialized = false;
    }
}
