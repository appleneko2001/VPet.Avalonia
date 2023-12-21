namespace VPet.Avalonia.Messages;

public class ShowMessageBoxMessage
{
    public string? Title;
    public string SupportingText = string.Empty;
    public Action? OnDialogClosed;
}