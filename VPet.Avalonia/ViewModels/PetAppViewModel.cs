using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using VPet.Avalonia.Enums;
using VPet.Avalonia.Messages;
using VPet.Avalonia.ViewModels.Commands;

namespace VPet.Avalonia.ViewModels;

public class PetAppViewModel : ReactiveObject
{

    private bool _hitThroughEnabled;

    public bool HitThroughEnabled
    {
        get => _hitThroughEnabled;
        set
        {
            this.RaiseAndSetIfChanged(ref _hitThroughEnabled, value);
            PropChanged?.Invoke(nameof(HitThroughEnabled), value);
        }
    }
    
    public ICommand ExitCommand => GlobalCommands.ExitCommand;

    // Workaround for solving avaloniaUI cannot change the bool property directly.
    public void OnSwitchHitThroughEnabled()
    {
        HitThroughEnabled = !HitThroughEnabled;
    }

    internal Action<string, object> PropChanged;
}