using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using VPet.Avalonia.Enums;
using VPet.Avalonia.Messages;
using VPet.Avalonia.Systems;
using VPet.Avalonia.ViewModels;
using VPet.Avalonia.ViewModels.Dialogs;
using VPet.Avalonia.Views.Windows;

namespace VPet.Avalonia.Views;

public partial class PetWidgetWindow : Window
{
    private bool _movedWhileDragging;
    private Point? _mousePressPoint;
    private Point? _mousePrevPoint;
    private ManualResetEventSlim _windowOpenWait = new();

    internal IntPtr _windowPointer;
    
    public PetWidgetWindow()
    {
        InitializeComponent();
        
        //this.AttachDevTools();
        
        PositionChanged += OnPositionChanged;
        Opened += OnWindowOpened;
        
        EventBus.Current.Listen<ShowMessageBoxMessage>(OnReceiveShowMessageBoxRequest);

        var handle = TryGetPlatformHandle();
        _windowPointer = handle?.Handle ?? IntPtr.Zero;
    }

    private void OnWindowOpened(object sender, EventArgs e)
    {
        _windowOpenWait?.Set();
    }

    private void OnReceiveShowMessageBoxRequest(ShowMessageBoxMessage msg)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (!_windowOpenWait.IsSet)
                _windowOpenWait.Wait();

            var window = new MessageBoxDialogWindow
            {
                DataContext = new MessageBoxDialogViewModel(msg)
            };
            
            window.ShowDialog(this);

            //ShowDialog();
        });
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        RestoreChanges();
        
        base.OnSizeChanged(e);
    }

    protected override void OnResized(WindowResizedEventArgs e)
    {
        RestoreChanges();
        
        base.OnResized(e);
    }

    private void RestoreChanges()
    {
        WindowState = WindowState.Normal;
        
        if(DataContext is not PetWidgetViewModel vm)
            return;

        Width = vm.WidgetSize;
        Height = vm.WidgetSize;

        //Position = vm.WidgetPosition;
    }
    
    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        //RestoreChanges();
    }

    private void OnPointerEnteredToWindow(object? sender, PointerEventArgs e)
    {
        if(DataContext is not PetWidgetViewModel vm)
            return;

        vm.ToolbarOpacity = 1;
    }

    private void OnPointerExitedFromWindow(object? sender, PointerEventArgs e)
    {
        if(DataContext is not PetWidgetViewModel vm)
            return;
        
        vm.ToolbarOpacity = 0;
    }

    private void SpriteSheetPlayer_OnReachToEnd(object? sender, EventArgs e)
    {
        if(DataContext is not PetWidgetViewModel vm)
            return;

        vm.OnCurrentSpritePlaybackCompleted();
    }

    private void Player_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if(sender is not Control control)
            return;

        _mousePressPoint = e.GetPosition(control);
    }

    private void Player_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if(sender is not Control control)
            return;

        if (!IsMousePressedDownPrivate())
            return;
        
        var cursorLoc = e.GetPosition(control);
        var moving = cursorLoc != _mousePrevPoint;

        if (moving)
            _movedWhileDragging = true;
            
        EventBus.Current.Post(new InteractToPetMessage
        {
            Action = InteractPetActionKind.StartDrag,
            Params = moving
        });
            
        var delta = cursorLoc - _mousePressPoint!.Value;
        var loc = this.PointToScreen(delta);
        TryToMoveWindow(loc);
        _mousePrevPoint = cursorLoc;
    }

    private void Player_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _mousePressPoint = null;

        if (!_movedWhileDragging)
            return;
        
        _movedWhileDragging = false;
        EventBus.Current.Post(new InteractToPetMessage
        {
            Action = InteractPetActionKind.EndDrag
        });
    }

    private void TryToMoveWindow(PixelPoint pointToScreen)
    {
        Position = pointToScreen;
        
        if(DataContext is not PetWidgetViewModel vm)
            return;

        vm.WidgetPosition = pointToScreen;
    }

    private bool IsMousePressedDownPrivate() => _mousePressPoint is not null;
}