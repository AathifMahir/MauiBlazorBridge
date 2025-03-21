#if ANDROID || IOS || WINDOWS || MACCATALYST

namespace MauiBlazorBridge.Services;
internal sealed class Bridge : IBridge
{
    public Framework Framework { get; private set; } = Framework.Maui;

    public PlatformIdentity Platform { get; private set; } = GetPlatform();

    public string PlatformVersion { get; private set; } = DeviceInfo.Version.ToString();

    public event EventHandler<PlatformIdentity>? PlatformChanged;

    private bool _isInitialized;
    public Task InitializeAsync()
    {
        if (_isInitialized) return Task.CompletedTask;

        Platform = GetPlatform();
        PlatformChanged?.Invoke(this, Platform);
        _isInitialized = true;

        return Task.CompletedTask;
    }

    private static PlatformIdentity GetPlatform()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
            return PlatformIdentity.Android;
        else if (DeviceInfo.Platform == DevicePlatform.iOS)
            return PlatformIdentity.IOS;
        else if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            return PlatformIdentity.Mac;
        else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            return PlatformIdentity.Windows;
        else
            return PlatformIdentity.Unknown;
    }
}
#endif