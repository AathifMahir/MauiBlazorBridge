namespace MauiBlazorBridge;
public sealed class MauiBlazorBridgeException : Exception
{
    public MauiBlazorBridgeException(string message) : base(message){}
    public MauiBlazorBridgeException(string code, string message) : base($"{code}: {message}"){}
}
