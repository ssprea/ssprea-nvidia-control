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
        });

        WindowsManager.AllWindows.Add(this);

        LoadGuiGrid();
        

    }

    private async Task DownloadDefaultGuiAsync()
    {
        Console.WriteLine("Default GUI does not exist! Downloading from https://gist.githubusercontent.com/ssprea/d16e12733bca0db0107ccd51e4dfa4c3/raw/73295ca1ed0dda84e9c2877aa782d280b2dbc1ae/MainWindowMainGrid.axaml");
        using (var c = new HttpClient())
        {
            try
            {
                var response = await c.GetAsync(
                    "https://gist.githubusercontent.com/ssprea/d16e12733bca0db0107ccd51e4dfa4c3/raw/73295ca1ed0dda84e9c2877aa782d280b2dbc1ae/MainWindowMainGrid.axaml");
                    
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
        
        if (!File.Exists($"{Program.DefaultDataPath}/Guis/Default/MainWindowMainGrid.axaml"))
        {
            if (!Directory.Exists($"{Program.DefaultDataPath}/Guis/Default/"))
            {
                Directory.CreateDirectory($"{Program.DefaultDataPath}/Guis/Default/");
            }
            Task.Run(DownloadDefaultGuiAsync).Wait();
        }
        
        //read selected gui name
        if (!File.Exists(Program.DefaultDataPath+"/SelectedGui.txt") || File.ReadAllText(Program.DefaultDataPath+"/SelectedGui.txt") == string.Empty)
        {
            File.WriteAllText(Program.DefaultDataPath+"/SelectedGui.txt", "Default");
        }
        
        var selectedGuiName = (File.ReadAllText(Program.DefaultDataPath+"/SelectedGui.txt")).Trim();
        
        
        
        //check if folder exists
        if (!File.Exists(Program.DefaultDataPath + "/Guis/" + selectedGuiName+"/MainWindowMainGrid.axaml"))
        {
            Console.WriteLine("No GUI named \""+selectedGuiName+"\" found. Loading default.");
            selectedGuiName = "Default";
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