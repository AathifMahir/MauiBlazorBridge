using Microsoft.JSInterop;

namespace MauiBlazorBridge.Maui.Sample;
public sealed class PromptService : BridgeFrameworkHandlerAsync
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    public PromptService(IBridge bridge, IJSRuntime jSRuntime) : base(bridge)
    {
        _jsRuntime = jSRuntime;
        _moduleTask = new(_jsRuntime.InvokeAsync<IJSObjectReference>("./hello.js").AsTask());
    }

    protected async override Task HandleBlazorAsync()
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("prompt", "Hello from Blazor");
    }

    protected override Task HandleMauiAsync()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (Application.Current is null || Application.Current.MainPage is null) return;
            await Application.Current.MainPage.DisplayAlert("Hello from Maui", "Hello from Maui", "OK");
        });
        return Task.CompletedTask;
    }
}
