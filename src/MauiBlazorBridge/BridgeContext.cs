using MauiBlazorBridge.Common;
using MauiBlazorBridge.Common.Exceptions;

namespace MauiBlazorBridge;
public sealed class BridgeContext
{
    private readonly IBridge _bridge;
    public event Action<DeviceFormFactor>? OnChanged;

    private BridgeContext(IBridge bridge)
    {
        _bridge = bridge;
        _bridge.IdiomChanged += Bridge_IdiomChanged;
    }

    private void Bridge_IdiomChanged(object? sender, DeviceFormFactor e)
    {
        OnChanged?.Invoke(e);
    }

    public static BridgeContext Create(IBridge bridge, Action<DeviceFormFactor> onIdiomChangedCallback)
    {
        if(!bridge.IsListening)
            throw new MauiBlazorBridgeException("Bridge is not listening for Idiom or Device Form Factor changes. Make sure to enable listener in BridgeContainer");

        var bridgeContext = new BridgeContext(bridge);
        bridgeContext.OnChanged += onIdiomChangedCallback;
        return bridgeContext;
    }

    public void Dispose()
    {
        _bridge.IdiomChanged -= Bridge_IdiomChanged;
    }
}
