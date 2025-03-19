
namespace MauiBlazorBridge;
public sealed class BridgeContext : IAsyncDisposable
{
    private readonly IBridge _bridge;
    public event Action<DeviceFormFactor>? OnChanged;

    private readonly ListenerType _listenerType;

    private BridgeContext(IBridge bridge)
    {
        _bridge = bridge;
        _listenerType = bridge.ListenerType;
        _bridge.FormFactorChanged += Bridge_IdiomChanged;
    }

    private void Bridge_IdiomChanged(object? sender, DeviceFormFactor e)
    {
        OnChanged?.Invoke(e);
    }

    /// <summary>
    /// This is Used for Creating a BridgeContext and Listening to Changes in the Form Factor of the Device
    /// </summary>
    /// <param name="bridge">Current Bridge Object</param>
    /// <param name="onIdiomChangedCallback">Delegate for Notifying the UI</param>
    /// <returns></returns>

    public async static Task<BridgeContext> CreateAsync(IBridge bridge, Action<DeviceFormFactor> onIdiomChangedCallback)
    {
        var bridgeContext = new BridgeContext(bridge);

        if (bridgeContext._listenerType is ListenerType.None)
            await bridge.InitializeListenerAsync();

        bridgeContext.OnChanged += onIdiomChangedCallback;
        bridge.FormFactorChanged?.Invoke(bridge, bridge.DeviceFormFactor);
        return bridgeContext;
    }


    /// <summary>
    /// This is Used for Disposing Whole BridgeContext and Stopping Listening to Changes in the Form Factor of the Device
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_listenerType is ListenerType.None)
            await _bridge.DisposeListener();

        _bridge.FormFactorChanged -= Bridge_IdiomChanged;
    }
}
