#if ANDROID || IOS || WINDOWS || MACCATALYST

namespace MauiBlazorBridge.Services;
internal sealed class BridgeFormFactor : IBridgeFormFactor
{
    bool _isInitialized = false;
    int _listenerCount = 0;
    CancellationTokenSource _cts = new();
    ChangeListeningMode _listeningMode = ChangeListeningMode.None;
    public DeviceFormFactor DeviceFormFactor { get; private set; } = DeviceFormFactor.UnknownState();

    public event EventHandler<DeviceFormFactor>? FormFactorChanged;

    public BridgeFormFactor()
    {
        DeviceFormFactor = GetFormFactor();
    }

    public Task InitializeAsync(ChangeListeningMode listenerType = ChangeListeningMode.None)
    {
        if(_isInitialized) return Task.CompletedTask;

        _listeningMode = listenerType;

        DeviceFormFactor = GetFormFactor();

        _isInitialized = true;

        if (listenerType is ChangeListeningMode.Global)
             CreateAsync();

        return Task.CompletedTask;
    }

    private static DeviceFormFactor GetFormFactor()
    {
        if (Application.Current is null || Application.Current.Windows.Count is 0) 
            return DeviceFormFactor.UnknownState();

        var width = Application.Current.Windows[0].Width;
        var height = Application.Current.Windows[0].Height;

        if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            return new DeviceFormFactor(FormFactor.Mobile, width, height);
        else if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            return new DeviceFormFactor(FormFactor.Tablet, width, height);
        else if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
            return new DeviceFormFactor(FormFactor.Desktop, width, height);
        else
            return DeviceFormFactor.UnknownState();
    }

    public Task CreateAsync()
    {
        if (!_isInitialized)
            throw new MauiBlazorBridgeException("Bridge is not initialized. Make sure to add BridgeProvider Component");

        if (_listeningMode is ChangeListeningMode.Suppressed) return Task.CompletedTask;

        OnNewListener();

        if (_listenerCount > 0 || _listeningMode is ChangeListeningMode.Suppressed)
        {
            _listenerCount++;
            return Task.CompletedTask;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Application.Current is null || Application.Current.Windows.Count is 0) return;
            Application.Current.Windows[0].SizeChanged += WindowSizeChanged;
        });

        _listenerCount++;

        return Task.CompletedTask;
    }

    private void WindowSizeChanged(object? sender, EventArgs e)
    {
        if (Application.Current is null || Application.Current.Windows.Count is 0) return;

        var width = Application.Current.Windows[0].Width;
        var height = Application.Current.Windows[0].Height;

        DeviceFormFactor newFormFactor;

        if (width <= 767)
            newFormFactor = new DeviceFormFactor(FormFactor.Mobile, width, height);
        else if (width >= 768 && width <= 1023)
            newFormFactor = new DeviceFormFactor(FormFactor.Tablet, width, height);
        else if (width >= 1024)
            newFormFactor = new DeviceFormFactor(FormFactor.Desktop, width, height);
        else
            newFormFactor = DeviceFormFactor.UnknownState(width, height);

        if (newFormFactor.FormFactor != DeviceFormFactor.FormFactor)
        {
            DeviceFormFactor = newFormFactor;
            FormFactorChanged?.Invoke(this, newFormFactor);
        }
    }

    private void OnNewListener()
    {
        _cts.Cancel();
        _cts = new();
    }

    public async ValueTask DisposeFormFactor()
    {
        try
        {
            if (_listeningMode is not ChangeListeningMode.None) return;

            if (_listenerCount is 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), _cts.Token);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (Application.Current is null || Application.Current.Windows.Count is 0) return;
                    Application.Current.Windows[0].SizeChanged -= WindowSizeChanged;
                });

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
#endif
