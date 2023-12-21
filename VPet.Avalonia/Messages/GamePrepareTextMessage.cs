namespace VPet.Avalonia.Messages;

public class GamePrepareTextMessage
{
    public string? Text;
    public bool IsComplete;

    public object[] Params = Array.Empty<object>();
}