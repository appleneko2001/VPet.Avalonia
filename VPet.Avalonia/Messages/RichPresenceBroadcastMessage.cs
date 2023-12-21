namespace VPet.Avalonia.Messages;

/// <summary>
/// Use this message if you want to change users rich presence status.
/// </summary>
public class RichPresenceBroadcastMessage
{
    public string StatusText = string.Empty;
}