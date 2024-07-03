namespace MauiBlazorBridge;
public enum ListenerType
{
    /// <summary>
    /// Set Listener to None, Therefore No Listener will be attached to the Bridge at Initialization and that Components Needs to Listen to Changes Can Attach Their Own Listener
    /// </summary>
    None,

    /// <summary>
    /// Set Listener to global, therefore single instance of Listener Available Across the App and Listen for Changes in All the time
    /// </summary>
    Global,

    /// <summary>
    /// Set Listener to Suppresed, Therefore Listener will not be listening for Changes in Real Time, This is useful when you want to listen for changes only once When Initializing the Bridge, When Suppressed Even the Components Can't Attah Their Own Listener
    /// </summary>
    Suppressed,
}
