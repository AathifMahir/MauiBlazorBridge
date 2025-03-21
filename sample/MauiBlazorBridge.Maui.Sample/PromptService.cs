using Microsoft.JSInterop;

namespace MauiBlazorBridge.Maui.Sample;
public sealed class PromptService : BridgeFrameworkHandler<Task>
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    public PromptService(IBridge bridge, IJSRuntime jSRuntime) : base(bridge)
    {
        _jsRuntime = jSRuntime;
        _moduleTask = new(_jsRuntime.InvokeAsync<IJSObjectReference>("./hello.js").AsTask());
    }

    protected async override Task HandleBlazor()
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("prompt", "Hello from Blazor");
    }

    protected override Task HandleMaui()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (Application.Current is null || Application.Current.Windows.Count is 0 || Application.Current.Windows[0].Page is null) return;
            await Application.Current.Windows[0].Page?.DisplayAlert("Hello from Maui", "Hello from Maui", "OK");
        });
        return Task.CompletedTask;
    }
}
