using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using sspreaNvidiaControl.ViewModels;

namespace sspreaNvidiaControl.Views;

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