using ReactiveUI;

namespace VPet.Avalonia.ViewModels.Overlays;

public class GamePrepViewModel : ReactiveObject
{
    public string Text
    {
        get => _text;
        internal set => this.RaiseAndSetIfChanged(ref _text, value);
    }
    
    private string _text = string.Empty;
}