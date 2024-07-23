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
    protected virtual T HandleMaui() =>
        default!;

    /// <summary>
    /// Handles the Blazor framework.
    /// </summary>
    /// <returns>A type of Generic Object</returns>
    protected virtual T HandleBlazor() =>
        default!;

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
    protected virtual void HandleMaui() { }

    /// <summary>
    /// Handles the Blazor framework.
    /// </summary>
    protected virtual void HandleBlazor() { }

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

public abstract class BridgeFrameworkHandlerAsync<T>
{
    private readonly IBridge _bridge;

    protected BridgeFrameworkHandlerAsync(IBridge bridge) =>
        _bridge = bridge;

    /// <summary>
    /// Handles the Maui framework asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task<T> HandleMauiAsync() =>
        Task.FromResult(default(T)!);

    /// <summary>
    /// Handles the Blazor framework asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task<T> HandleBlazorAsync() =>
        Task.FromResult(default(T)!);

    /// <summary>
    /// Executes the appropriate framework handler asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="MauiBlazorBridgeException">Thrown when the framework identifier is unknown.</exception>
    public Task<T> ExecuteAsync() =>
        _bridge.Framework switch
        {
            Framework.Maui => HandleMauiAsync(),
            Framework.Blazor => HandleBlazorAsync(),
            _ => throw new MauiBlazorBridgeException("Framework Identifier is Unknown, Make sure you have Initialized the Bridge Using BridgeProvider"),
        };

}

public abstract class BridgeFrameworkHandlerAsync
{
    private readonly IBridge _bridge;

    protected BridgeFrameworkHandlerAsync(IBridge bridge) =>
        _bridge = bridge;

    /// <summary>
    /// Handles the Maui framework asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task HandleMauiAsync() =>
        Task.CompletedTask;

    /// <summary>
    /// Handles the Blazor framework asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task HandleBlazorAsync() =>
        Task.CompletedTask;

    /// <summary>
    /// Executes the appropriate framework handler asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="MauiBlazorBridgeException">Thrown when the framework identifier is unknown.</exception>
    public async Task ExecuteAsync()
    {
        switch(_bridge.Framework)
        {
            case Framework.Maui:
                await HandleMauiAsync();
                break;
            case Framework.Blazor:
                await HandleBlazorAsync();
                break;
            default:
                throw new MauiBlazorBridgeException("Framework Identifier is Unknown, Make sure you have Initialized the Bridge Using BridgeProvider");
        }
    }
}
