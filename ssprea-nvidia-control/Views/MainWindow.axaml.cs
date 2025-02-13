using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

        Activated += (s, e) =>
        {
            ViewModel!.WindowLoadedHandler();
        };
        
        this.WhenActivated(action =>
        {
            action(ViewModel!.ShowOcProfileDialog.RegisterHandler(DoShowNewProfileDialogAsync));

            action(ViewModel!.ShowFanCurveEditorDialog.RegisterHandler(DoShowFanCurveEditorDialogAsync));
            
            action(ViewModel!.ShowSudoPasswordRequestDialog.RegisterHandler(DoShowSudoPasswordRequestDialogAsync));
            
            action(ViewModel!.ShowSettingsDialog.RegisterHandler(DoShowSettingsDialogAsync));
        });

        WindowsManager.AllWindows.Add(this);

        LoadGuiGrid();
        

    }

    private async Task DownloadDefaultGuiAsync()
    {
        const string requestUrl =
            "https://gist.githubusercontent.com/ssprea/d16e12733bca0db0107ccd51e4dfa4c3/raw/73295ca1ed0dda84e9c2877aa782d280b2dbc1ae/MainWindowMainGrid.axaml";
        
        Console.WriteLine("Default GUI does not exist! Downloading from "+requestUrl);
        using (var c = new HttpClient())
        {
            try
            {
                var response = await c.GetAsync(
                    requestUrl);
                    
                var download = await response.Content.ReadAsStringAsync();
                
                await File.WriteAllTextAsync($"{Program.DefaultDataPath}/Guis/Default/MainWindowMainGrid.axaml", download);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine($"Automatic download failed! Try downloading the file manually and put it in: {Program.DefaultDataPath}/Guis/Default/MainWindowMainGrid.axaml");
                Environment.Exit(1);
            }


                
        }
    }
    
    private void LoadGuiGrid()
    {
        if (!Directory.Exists($"{Program.DefaultDataPath}/Guis/"))
            Directory.CreateDirectory($"{Program.DefaultDataPath}/Guis/");
        
        
        // if (!File.Exists($"{Program.DefaultDataPath}/Guis/Default/MainWindowMainGrid.axaml"))
        // {
        //     if (!Directory.Exists($"{Program.DefaultDataPath}/Guis/Default/"))
        //     {
        //         Directory.CreateDirectory($"{Program.DefaultDataPath}/Guis/Default/");
        //     }
        //     Task.Run(DownloadDefaultGuiAsync).Wait();
        // }
        
        //read selected gui name
        if (!File.Exists(Program.DefaultDataPath+"/SelectedGui.txt") || File.ReadAllText(Program.DefaultDataPath+"/SelectedGui.txt") == string.Empty)
        {
            File.WriteAllText(Program.DefaultDataPath+"/SelectedGui.txt", "Default");
        }
        
        var selectedGuiName = (File.ReadAllText(Program.DefaultDataPath+"/SelectedGui.txt")).Trim();
        if (selectedGuiName == "Default")
        {
            Console.WriteLine("Using default gui, skipping loading from file");
            return;
        }
        
        
        //check if folder exists
        if (!File.Exists(Program.DefaultDataPath + "/Guis/" + selectedGuiName+"/MainWindowMainGrid.axaml"))
        {
            Console.WriteLine("No GUI named \""+selectedGuiName+"\" found. Loading default.");
            selectedGuiName = "Default";
            return;
        }

        var mainGridLoadPath = $"{Program.DefaultDataPath}/Guis/{selectedGuiName}/MainWindowMainGrid.axaml";
        
        var guiStream = File.OpenRead(mainGridLoadPath);
        var loadedGrid = AvaloniaRuntimeXamlLoader.Load(guiStream) as Grid;
        
        
        MainOcWindow.Content =  loadedGrid;
        Console.WriteLine("Successfully loaded Gui from: "+mainGridLoadPath);
        //Console.WriteLine("Successfully loaded Gui from: "+mainGridLoadPath);
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
    
    private async Task DoShowSettingsDialogAsync(IInteractionContext<SettingsMainWindowViewModel, object?> interaction)
    {
        var dialog = new SettingsMainWindow()
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<object?>(this);
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
    

    private void Window_OnClosed(object? sender, EventArgs e)
    {
        MainWindowViewModel.NvmlService.Shutdown();
    }

    
}