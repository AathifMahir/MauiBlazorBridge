#if ANDROID || IOS || WINDOWS || MACCATALYST

namespace MauiBlazorBridge.Services;
internal sealed class BridgeConnectivity : IBridgeConnectivity
{
    bool _isInitialized = false;
    public BridgeConnectivity()
    {
        IsInternetConnected = Connectivity.NetworkAccess == NetworkAccess.Internet;
    }

    public bool IsInternetConnected { get; private set; } = true;

    public event EventHandler<bool>? InternetConnectionChanged;

    public Task InitializeAsync(int internetConnectionInvervalInSeconds = 10)
    {
        if (_isInitialized) return Task.CompletedTask;

        Connectivity.ConnectivityChanged += OnConnectivityChanged;
        _isInitialized = true;

        return Task.CompletedTask;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (IsInternetConnected != (e.NetworkAccess == NetworkAccess.Internet))
        {
            IsInternetConnected = e.NetworkAccess == NetworkAccess.Internet;
            InternetConnectionChanged?.Invoke(this, IsInternetConnected);
        }
    }
}

#endif
