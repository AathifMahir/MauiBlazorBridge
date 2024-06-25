namespace MauiBlazorBridge;
public interface IBridge
{
    /// <summary>
    /// This is Used to Determine the Framework of the Application, Whether it is Blazor or Maui
    /// </summary>
    FrameworkIdentity Framework { get; }

    /// <summary>
    /// This is Used for Determining the Platform of the Application, Whether it is Android, iOS, Windows, or Mac
    /// </summary>
    PlatformIdentity Platform { get; }

    /// <summary>
    /// This is Used for Determining the Version of the Platform, This would be Unknown on Blazor
    /// </summary>
    string PlatformVersion { get; }

    /// <summary>
    /// This is Used for Determining the Form Factor of the Device, Whether it is Desktop, Tablet, or Mobile
    /// </summary>
    DeviceFormFactor FormFactor {  get; }

    /// <summary>
    /// This is Used for Listening to Changes in the Form Factor of the Device and Internally Used by the BridgeContext
    /// </summary>
    EventHandler<DeviceFormFactor>? FormFactorChanged { get; set; }

    /// <summary>
    /// Platform Changed is Mainly Used for Scenario Where PreRendering is Enabled, In Most Cases Use the Platform Property
    /// </summary>
    EventHandler<PlatformIdentity>? PlatformChanged { get; set; }

    /// <summary>
    /// This is Used for Initializing the Bridge and used Internally on BridgeProvider
    /// </summary>
    /// <param name="isListenerEnabled">
    /// Determines Whether the Bridge Should Listen to Changes in the Form Factor of the Device Globally
    /// </param>
    Task InitializeAsync(bool isListenerEnabled = false);

    /// <summary>
    /// This is Used for Initializing the Listener and Used Internally by the BridgeContext
    /// </summary>
    /// <returns></returns>
    Task InitializeListenerAsync();

    /// <summary>
    /// This is Used for Disposing the Listener and Used Internally by the BridgeContext
    /// </summary>
    ValueTask DisposeListener();

    /// <summary>
    /// This is Used for Identifying Whether the Bridge is Listening to Changes in the Form Factor of the Device Globally
    /// </summary>
    bool IsListening { get; }
}
