using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using VPet.Avalonia.Debugging;
using VPet.Avalonia.Interops;
using VPet.Avalonia.Primitives;
using VPet.Avalonia.Services;
using VPet.Avalonia.ViewModels;
using VPet.Avalonia.Views;

namespace VPet.Avalonia;

public partial class PetApp : Application
{
    private PetAppViewModel _viewModel;
    private PlatformSpecificBridge _interopImpl;
    private PetGameLogicService _gameLogic;
    private IntPtr _widgetPointer;
    
    internal static string ApplicationRootPath { get; private set; }
    
    public PetApp()
    {
        //RxApp.DefaultExceptionHandler = new 
        
        var programLocation = Assembly.GetEntryAssembly()?.Location ??
                              throw new InvalidOperationException("Unable to find the executable session.");
        ApplicationRootPath = Path.GetDirectoryName(programLocation) ??
                              throw new InvalidOperationException("Unable to get application root path.");
        
        _gameLogic = new PetGameLogicService();
        DebuggerConsole.SwitchDebug(true);

        var options = new OptionsTable();
        options[""] = new object();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != DataContextProperty)
            return;

        if (change.NewValue is PetAppViewModel vm)
        {
            _viewModel = vm;
            vm. PropChanged = OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(string name, object newValue)
    {
        if (name == nameof(PetAppViewModel.HitThroughEnabled))
        {
            _interopImpl.SetWindowHitThrough(_widgetPointer, (bool) newValue);
        }
    }

    public void SetPlatformSpecificBridge(PlatformSpecificBridge bridge)
    {
        _interopImpl = bridge;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // If the application runs at desktop machine
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Should open pet widget first.
            var widget = new PetWidgetWindow
            {
                Topmost = true
            };
            
            widget.Opened += OncePetWindowOpened;

            desktop.MainWindow = widget;

            _widgetPointer = widget._windowPointer;

            var queue = new Queue<KeyValuePair<string, Action>>(new[]
            {
                new KeyValuePair<string, Action>("Window hit-test mask",
                    () => _interopImpl.SetWindowTransparentHitThrough(widget._windowPointer))
            });

            while (queue.TryDequeue(out var item))
            {
                try
                {
                    item.Value?.Invoke();
                }
                catch (Exception e)
                {
                    this.WriteLine(MessageSeverity.Error, 
                        $"Unable to execute optional work \"{item.Key}\". {e}");
                }
            }
        }
        
        base.OnFrameworkInitializationCompleted();
    }

    private void OncePetWindowOpened(object? sender, EventArgs e)
    {
        if(sender is not Window w)
            return;

        w.Opened -= OncePetWindowOpened;
        
        _gameLogic.Init();
    }
}