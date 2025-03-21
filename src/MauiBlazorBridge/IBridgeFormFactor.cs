namespace MauiBlazorBridge;
public interface IBridgeFormFactor
{
    /// <summary>
    /// This is Used for Initializing the Bridge and used Internally on BridgeProvider
    /// </summary>
    /// <param name="listeningMode">
    /// Determines Whether the Bridge Should Listen to Changes in Globally, Suppressed or None
    /// </param>
    Task InitializeAsync(ChangeListeningMode listeningMode = ChangeListeningMode.None);

    /// <summary>
    /// This is Used for Initializing the Listener and Used Internally by the BridgeContext
    /// </summary>
    /// <returns></returns>
    Task CreateAsync();

    /// <summary>
    /// This is Used for Disposing the Listener and Used Internally by the BridgeContext
    /// </summary>
    ValueTask DisposeFormFactor();

    /// <summary>
    /// This is Used for Determining the Form Factor of the Device, Whether it is Desktop, Tablet, or Mobile
    /// </summary>
    DeviceFormFactor DeviceFormFactor { get; }

    /// <summary>
    /// This is Used for Listening to Changes in the Form Factor of the Device and Internally Used by the BridgeContext
    /// </summary>
    event EventHandler<DeviceFormFactor>? FormFactorChanged;
}
