#pragma warning disable CS1998
#pragma warning disable IDE0051
using Microsoft.JSInterop;

namespace MauiBlazorBridge.Services;
public sealed class BridgeWeb : IBridge, IAsyncDisposable
{

    private readonly Lazy<Task<IJSObjectReference>> moduleTask;
    bool _isInitialized = false;
    private readonly DotNetObjectReference<BridgeWeb> _dotNetObjectReference;

    const string _releasePath = "./_content/AathifMahir.MauiBlazor.MauiBlazorBridge/Bridge.js";
    const string _debugPath = "./Bridge.js";

    public Framework Framework { get; private set; } = GetFrameworkIdentity();
    public PlatformIdentity Platform { get; private set; } = PlatformIdentity.Unknown;

    public event EventHandler<PlatformIdentity>? PlatformChanged;
    public string PlatformVersion { get; private set; } = GetPlatformVersion();

    public BridgeWeb(IJSRuntime jsRuntime)
    {

#if DEBUG && ANDROID || DEBUG && IOS || DEBUG && WINDOWS || DEBUG && MACCATALYST
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", _debugPath).AsTask());
#else
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", _releasePath).AsTask());
#endif

        _dotNetObjectReference = DotNetObjectReference.Create(this);

    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        var module = await moduleTask.Value ?? throw new MauiBlazorBridgeException("Failed to import the Bridge.js");

        Platform = await GetPlatformAsync(module);
        PlatformChanged?.Invoke(this, Platform);

        _isInitialized = true;
    }


    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            _dotNetObjectReference.Dispose();
            await module.DisposeAsync();
        }
    }


    private static async ValueTask<PlatformIdentity> GetPlatformAsync(IJSObjectReference module)
    {
        if (Enum.TryParse<PlatformIdentity>(await module.InvokeAsync<string>("getPlatform"), out var platformIdentity))
            return platformIdentity;

        return PlatformIdentity.Unknown;
    }


    private static Framework GetFrameworkIdentity() => Framework.Blazor;
    private static string GetPlatformVersion() =>  "Unknown";

}
#pragma warning restore CS1998
#pragma warning restore IDE0051