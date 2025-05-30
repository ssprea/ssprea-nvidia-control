using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Newtonsoft.Json.Linq;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.NVML;
using ssprea_nvidia_control.ViewModels;
using ReactiveUI;

namespace ssprea_nvidia_control.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
            
// #if DEBUG
//             this.AttachDevTools();
// #endif
            
        Closing += (s, e) =>
        {
            if (s is null)
                return;
            
            ((Window)s).Hide();
            e.Cancel = true;
        };

        Activated += async (s, e) =>
        {
            await ViewModel!.CheckAndApplyAutoApplyProfile();
        };
        
        this.WhenActivated(action =>
        {
            action(ViewModel!.ShowOcProfileDialog.RegisterHandler(DoShowNewProfileDialogAsync));

            action(ViewModel!.ShowFanCurveEditorDialog.RegisterHandler(DoShowFanCurveEditorDialogAsync));
            
            action(ViewModel!.ShowSudoPasswordRequestDialog.RegisterHandler(DoShowSudoPasswordRequestDialogAsync));
        });

        Opened += async (s, e) =>
        {
            
            //await ViewModel!.CheckDependencies();
        };

        Loaded += async (s, e) =>
        {
            await ViewModel!.LoadedEvent();


        };
        
        WindowsManager.AllWindows.Add(this);
        
    }
    
   
    
    private async Task DoShowNewProfileDialogAsync(IInteractionContext<NewOcProfileWindowViewModel, OcProfile?> interaction)
    {
        var dialog = new NewOcProfileWindow
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<OcProfile?>(this);
        interaction.SetOutput(result);
    }
    
    private async Task DoShowFanCurveEditorDialogAsync(IInteractionContext<FanCurveEditorWindowViewModel, FanCurveViewModel?> interaction)
    {
        var dialog = new FanCurveEditorWindow
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<FanCurveViewModel?>(this);
        interaction.SetOutput(result);
    }
    
    private async Task DoShowSudoPasswordRequestDialogAsync(IInteractionContext<SudoPasswordRequestWindowViewModel, SudoPassword?> interaction)
    {
        var dialog = new SudoPasswordRequestWindow
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<SudoPassword?>(this);
        interaction.SetOutput(result);
    }
    
    // private void InitializeComponent()
    // {
    //     AvaloniaXamlLoader.Load(this);
    //     var viewModel = DataContext as MainWindowViewModel;
    //     ComboBox comboBox = this.Find<ComboBox>("GpuSelector");
    //     comboBox.ItemsSource = viewModel.NvmlService.GpuList;
    //     comboBox.SelectedIndex = 0;
    // }


    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        
    }

    private void FanComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectedGpuFan = ((NvmlGpuFan)(e.AddedItems[0]));
    }
    


    private void Window_OnClosed(object? sender, EventArgs e)
    {
        MainWindowViewModel.NvmlService.Shutdown();
    }

    
}