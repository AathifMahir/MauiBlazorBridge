namespace MauiBlazorBridge;
public interface IBridgeConnectivity
{
    bool IsInternetConnected { get; }

    event EventHandler<bool>? InternetConnectionChanged;
    Task InitializeAsync(int internetConnectionInvervalInSeconds = 10);
}
