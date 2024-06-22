using MauiBlazorBridge.Common;

namespace MauiBlazorBridge;
public sealed class BridgeContext : IAsyncDisposable
{
    private readonly IBridge _bridge;
    public event Action<DeviceFormFactor>? OnChanged;

    private readonly bool _isGlobalListener = true;

    private BridgeContext(IBridge bridge)
    {
        _bridge = bridge;
        _isGlobalListener = bridge.IsListening;
        _bridge.FormFactorChanged += Bridge_IdiomChanged;
    }

    private void Bridge_IdiomChanged(object? sender, DeviceFormFactor e)
    {
        OnChanged?.Invoke(e);
    }

    public async static Task<BridgeContext> CreateAsync(IBridge bridge, Action<DeviceFormFactor> onIdiomChangedCallback)
    {
        var bridgeContext = new BridgeContext(bridge);

        if (!bridgeContext._isGlobalListener)
            await bridge.InitializeListenerAsync();

        bridgeContext.OnChanged += onIdiomChangedCallback;
        bridge.FormFactorChanged?.Invoke(bridge, bridge.FormFactor);
        return bridgeContext;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_isGlobalListener)
            await _bridge.DisposeListener();

        _bridge.FormFactorChanged -= Bridge_IdiomChanged;
    }
}
