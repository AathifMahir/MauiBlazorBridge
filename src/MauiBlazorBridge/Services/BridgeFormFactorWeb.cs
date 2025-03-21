
using Microsoft.JSInterop;
using System.Text.Json;

namespace MauiBlazorBridge.Services;
public sealed class BridgeFormFactorWeb : IBridgeFormFactor
{
    readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    const string _releasePath = "./_content/AathifMahir.MauiBlazor.MauiBlazorBridge/BridgeFormFactor.js";
    const string _debugPath = "./BridgeFormFactor.js";

    bool _isInitialized = false;
    ChangeListeningMode _listeningMode = ChangeListeningMode.None;
    CancellationTokenSource _cts = new();
    int _listenerCount = 0;

    readonly DotNetObjectReference<BridgeFormFactorWeb> _dotNetObjectReference;

    public BridgeFormFactorWeb(IJSRuntime jSRuntime)
    {
        _moduleTask = new(() => jSRuntime.InvokeAsync<IJSObjectReference>(
            "import",
#if DEBUG && ANDROID || DEBUG && IOS || DEBUG && WINDOWS || DEBUG && MACCATALYST
            _debugPath
#else
            _releasePath
#endif
            ).AsTask());

        _dotNetObjectReference = DotNetObjectReference.Create(this);

    }

    public DeviceFormFactor DeviceFormFactor { get; private set; } = DeviceFormFactor.UnknownState();

    public event EventHandler<DeviceFormFactor>? FormFactorChanged;

    public async Task InitializeAsync(ChangeListeningMode listenerType = ChangeListeningMode.None)
    {
        if(_isInitialized) return;

        _listeningMode = listenerType;

        var module = await _moduleTask.Value ?? throw new MauiBlazorBridgeException("Failed to import the BridgeFormFactor.js");

        DeviceFormFactor = await GetFormFactorAsync(module);

        _isInitialized = true;

        if (listenerType is ChangeListeningMode.Global)
            await CreateAsync();
    }

    public async Task CreateAsync()
    {
        if (!_isInitialized)
            throw new MauiBlazorBridgeException("Bridge is not initialized. Make sure to add BridgeFormFactorProvider Component");

        if (_listeningMode is ChangeListeningMode.Suppressed) return;

        OnNewListener();

        if (_listenerCount > 0 || _listeningMode is ChangeListeningMode.Suppressed)
        {
            _listenerCount++;
            return;
        }

        var module = await _moduleTask.Value ?? throw new MauiBlazorBridgeException("Failed to import the BridgeFormFactor.js");
        await module.InvokeVoidAsync("initialize", _dotNetObjectReference);

        _listenerCount++;
    }

    [JSInvokable]
    public ValueTask NotifyFormFactorChanged(string formFactorString)
    {
        if (!_isInitialized)
            throw new MauiBlazorBridgeException("BridgeFormFactor is not initialized.");

        var formFactor = JsonSerializer.Deserialize<DeviceFormFactor>(formFactorString);

        if (formFactor is not null && DeviceFormFactor != formFactor)
        {
            DeviceFormFactor = formFactor;
            FormFactorChanged?.Invoke(this, formFactor);
        }
        return ValueTask.CompletedTask;
    }

    private static async ValueTask<DeviceFormFactor> GetFormFactorAsync(IJSObjectReference module)
    {
        var formFactorString = await module.InvokeAsync<string>("getFormFactor");

        if (string.IsNullOrEmpty(formFactorString))
            return DeviceFormFactor.UnknownState();

        var formFactor = JsonSerializer.Deserialize<DeviceFormFactor>(formFactorString);
        return formFactor ?? DeviceFormFactor.UnknownState();
    }

    private void OnNewListener()
    {
        _cts.Cancel();
        _cts = new();
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;

            if (_listenerCount > 0 && _listeningMode is not ChangeListeningMode.Suppressed)
            {
                await module.InvokeVoidAsync("dispose");
            }

            _dotNetObjectReference.Dispose();
            await module.DisposeAsync();
        }
    }

    public async ValueTask DisposeFormFactor()
    {
        try
        {
            if (_listeningMode is not ChangeListeningMode.None) return;

            if (_listenerCount is 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), _cts.Token);

                if (_moduleTask.IsValueCreated)
                {
                    var module = await _moduleTask.Value;
                    await module.InvokeVoidAsync("dispose");
                }

                _listenerCount = 0;
            }
            else if (_listenerCount > 0)
                _listenerCount--;
        }
        catch (TaskCanceledException)
        {
            _listenerCount--;
            Console.WriteLine("DisposeListener Task was cancelled");
        }
    }

   

   
}
