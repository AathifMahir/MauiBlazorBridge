namespace MauiBlazorBridge;
public interface IBridge
{
    /// <summary>
    /// This is Used to Determine the Framework of the Application, Whether it is Blazor or Maui
    /// </summary>
    Framework Framework { get; }

    /// <summary>
    /// This is Used for Determining the Platform of the Application, Whether it is Android, iOS, Windows, or Mac
    /// </summary>
    PlatformIdentity Platform { get; }

    /// <summary>
    /// This is Used for Determining the Version of the Platform, This would be Unknown on Blazor
    /// </summary>
    string PlatformVersion { get; }

    /// <summary>
    /// Platform Changed is Mainly Used for Scenario Where PreRendering is Enabled, In Most Cases Use the Platform Property
    /// </summary>
    event EventHandler<PlatformIdentity>? PlatformChanged;

    /// <summary>
    /// This is Used for Initializing the Bridge and used Internally on BridgeProvider
    /// </summary>
    Task InitializeAsync();

}
