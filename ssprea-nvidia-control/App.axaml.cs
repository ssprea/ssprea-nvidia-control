using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
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
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
                
            };
            
            LiveCharts.Configure(config =>
                {
                    
                    config.HasTextSettings(new TextSettings()
                    {
                        DefaultTypeface = SKTypeface.FromStream(AssetLoader.Open(
                            new Uri("avares://ssprea-nvidia-control/Assets/Fonts/NotoSans/NotoSans-Light.ttf")))
                    });
                }
                // o SKFontManager.Default.MatchFamily("...")
            );
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