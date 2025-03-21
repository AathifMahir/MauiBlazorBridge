namespace MauiBlazorBridge;
public abstract class BridgeFrameworkHandler<T>
{
    private readonly IBridge _bridge;

    protected BridgeFrameworkHandler(IBridge bridge) =>
        _bridge = bridge;

    /// <summary>
    /// Handles the Maui framework.
    /// </summary>
    /// <returns>A type of Generic Object</returns>
    protected abstract T HandleMaui();

    /// <summary>
    /// Handles the Blazor framework.
    /// </summary>
    /// <returns>A type of Generic Object</returns>
    protected abstract T HandleBlazor();

    /// <summary>
    /// Executes the appropriate framework handler.
    /// </summary>
    /// <returns>A type of Generic Object</returns>
    /// <exception cref="MauiBlazorBridgeException">Thrown when the framework identifier is unknown.</exception>
    public T Execute() =>
        _bridge.Framework switch
        {
            Framework.Maui => HandleMaui(),
            Framework.Blazor => HandleBlazor(),
            _ => throw new MauiBlazorBridgeException("Framework Identifier is Unknown, Make sure you have Initialized the Bridge Using BridgeProvider"),
        };

}

public abstract class BridgeFrameworkHandler
{
    private readonly IBridge _bridge;

    protected BridgeFrameworkHandler(IBridge bridge) =>
        _bridge = bridge;

    /// <summary>
    /// Handles the Maui framework.
    /// </summary>
    protected abstract void HandleMaui();

    /// <summary>
    /// Handles the Blazor framework.
    /// </summary>
    protected abstract void HandleBlazor();

    /// <summary>
    /// Executes the appropriate framework handler.
    /// </summary>
    /// <exception cref="MauiBlazorBridgeException">Thrown when the framework identifier is unknown.</exception>
    public void Execute()
    {
        switch(_bridge.Framework)
        {
            case Framework.Maui:
                HandleMaui();
                break;
            case Framework.Blazor:
                HandleBlazor();
                break;
            default:
                throw new MauiBlazorBridgeException("Framework Identifier is Unknown, Make sure you have Initialized the Bridge Using BridgeProvider");
        }
    }
        

}


