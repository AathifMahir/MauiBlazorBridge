using MauiBlazorBridge.Common;

namespace MauiBlazorBridge;
public interface IBridge
{
    FrameworkIdentity Framework { get; }
    Common.PlatformIdentity Platform { get; }
    string PlatformVersion { get; }
    DeviceFormFactor FormFactor {  get; }
    EventHandler<DeviceFormFactor>? FormFactorChanged { get; set; }
    Task InitializeAsync(bool isListenerEnabled = false);
    Task InitializeListenerAsync();
    ValueTask DisposeListener();
    bool IsListening { get; }
}
