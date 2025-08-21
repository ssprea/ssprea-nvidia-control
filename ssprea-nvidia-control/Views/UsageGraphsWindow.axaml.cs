using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ssprea_nvidia_control.ViewModels;

namespace ssprea_nvidia_control.Views;

public partial class UsageGraphsWindow : ReactiveWindow<UsageGraphsWindowViewModel>
{
    public UsageGraphsWindow()
    {
        InitializeComponent();
        
    }


    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        ViewModel?.CancelTokenSrc.Cancel();
    }
}