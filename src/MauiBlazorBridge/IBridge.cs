using MauiBlazorBridge.Common;

namespace MauiBlazorBridge;
public interface IBridge
{
    FrameworkIdentity Framework { get; }
    Common.Platform Platform { get; }
    DeviceFormFactor Idiom {  get; }
    EventHandler<DeviceFormFactor>? IdiomChanged { get; set; }
    Task InitializeAsync(bool isIdiomListenerEnabled = false);
    bool IsListening { get; }
}
