using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using VPet.Avalonia.Enums;
using VPet.Avalonia.Messages;
using VPet.Avalonia.Systems;

namespace VPet.Avalonia.ViewModels.Commands;

internal class GlobalCommands
{
    internal static ICommand ExitCommand { get; } = ReactiveCommand.Create(() =>
    {
        void Impl()
        {
            if (Application.Current?.ApplicationLifetime is not ClassicDesktopStyleApplicationLifetime applicationLifetime)
            {
                throw new NotSupportedException("This application can be ran only in desktop env.");
            }

            EventBus.Current.Post(new StopServiceMessage());
            applicationLifetime.Shutdown();
        }

        EventBus.Current.Post(new StopViewModelMessage());
        EventBus.Current.Post(new TriggerBasicSequenceMessage
        {
            TargetState = PetActivityState.Shutdown,
            PostSequenceTask = Impl
        });
    });
}