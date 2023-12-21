using ReactiveUI;
using VPet.Avalonia.Messages;

namespace VPet.Avalonia.ViewModels.Dialogs;

public class MessageBoxDialogViewModel : ReactiveObject
{
    public MessageBoxDialogViewModel()
    {
        // Dummy view model
    }

    private Action? _onClickAction;
    
    public MessageBoxDialogViewModel(ShowMessageBoxMessage msg)
    {
        Title = msg.Title;
        SupportingText = msg.SupportingText;
        _onClickAction = msg.OnDialogClosed;
    }

    public string? Title { get; set; }
    
    public string? SupportingText { get; set; }
    
    public Action<object>? WindowClose { get; set; }

    public void OnClickButton()
    {
        WindowClose?.Invoke(true);
        _onClickAction?.Invoke();
    }
}