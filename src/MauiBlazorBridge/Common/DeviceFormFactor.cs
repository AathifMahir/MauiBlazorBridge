using System.Text.Json.Serialization;

namespace MauiBlazorBridge;
public sealed record DeviceFormFactor
{
    /// <summary>
    /// This is Used for Determining the Form Factor of the Device, Whether it is Desktop, Tablet, or Mobile
    /// </summary>
    /// 
    [JsonConverter(typeof(JsonStringEnumConverter<FormFactor>))]
    public FormFactor FormFactor { get; init; }

    /// <summary>
    /// This is Used for Determining the Width of the Form Factor of the Device
    /// </summary>
    public double Width { get; init; }

    /// <summary>
    /// This is Used for Determining the Height of the Form Factor of the Device
    /// </summary>
    public double Height { get; init; }

    public DeviceFormFactor(FormFactor formFactor, double width, double height)
    {
        FormFactor = formFactor;
        Width = width;
        Height = height;
    }
    public static DeviceFormFactor UnknownState() => new(FormFactor.Unknown, 0, 0);
    public static DeviceFormFactor UnknownState(double width, double height) => new(FormFactor.Unknown, width, height);
}
