using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using SLimit.Gui.ViewModels;

namespace SLimit.Gui.Views;

public partial class UsageGraphsWindow : ReactiveWindow<UsageGraphsWindowViewModel>
{
    public UsageGraphsWindow()
    {
        InitializeComponent();
        
    }


    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        ViewModel?.StopUpdating();
    }
}