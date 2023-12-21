using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using VPet.Avalonia.ViewModels.Dialogs;

namespace VPet.Avalonia.Views.Windows;

public partial class MessageBoxDialogWindow : Window
{
    public MessageBoxDialogWindow()
    {
        InitializeComponent();
        
        Closing += OnClosingWindow;
        Closed += OnClosedWindow;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, EventArgs e)
    {
        if (DataContext is not MessageBoxDialogViewModel vm)
            return;

        vm.WindowClose = Close;
    }

    private void OnClosingWindow(object sender, WindowClosingEventArgs e)
    {
        //throw new NotImplementedException();
    }

    private void OnClosedWindow(object sender, EventArgs e)
    {
        if (DataContext is not MessageBoxDialogViewModel vm)
            return;
        
        vm.OnClickButton();
    }
}