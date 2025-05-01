
namespace MauiBlazorBridge;
public sealed class BridgeFormFactorContext : IAsyncDisposable
{
    private readonly IBridgeFormFactor _bridge;
    public event Action<DeviceFormFactor>? OnChanged;

    private BridgeFormFactorContext(IBridgeFormFactor bridge)
    {
        _bridge = bridge;
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

    public async static Task<BridgeFormFactorContext> CreateAsync(IBridgeFormFactor bridge, Action<DeviceFormFactor> onIdiomChangedCallback)
    {
        var bridgeContext = new BridgeFormFactorContext(bridge);
        bridgeContext.OnChanged += onIdiomChangedCallback;
        
        await bridge.CreateAsync();

        return bridgeContext;
    }


    /// <summary>
    /// This is Used for Disposing Whole BridgeContext and Stopping Listening to Changes in the Form Factor of the Device
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _bridge.DisposeFormFactor();

        _bridge.FormFactorChanged -= Bridge_IdiomChanged;
    }
}
