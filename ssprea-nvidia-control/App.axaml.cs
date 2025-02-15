using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using ssprea_nvidia_control.ViewModels;
using ssprea_nvidia_control.Views;
using ssprea_nvidia_control.Models;

namespace ssprea_nvidia_control;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            
            
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
                
            };
            
            
        }
        
        
        

        base.OnFrameworkInitializationCompleted();
    }
    


    private void TrayIcon_OnClicked(object? sender, EventArgs e)
    {
        WindowsManager.AllWindows.FirstOrDefault(x => x.Name == "MainOcWindow").Show();
    }

    private void NativeMenuItem_OnClick(object? sender, EventArgs e)
    {
        Program.KillFanCurveProcess();
        Environment.Exit(0);
    }
}