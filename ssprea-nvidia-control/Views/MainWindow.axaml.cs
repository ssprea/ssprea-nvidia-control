using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.ViewModels;
using ReactiveUI;
using Serilog;

namespace ssprea_nvidia_control.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private int[]? _defaultGuiResolution;
    private Grid? _defaultGuiGrid;
    
    public MainWindow()
    {
        InitializeComponent();
            
#if DEBUG
            this.AttachDevTools();
#endif
            
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownRequested += (sender, e) =>
            {
                File.Delete(Program.DefaultDataPath+"/.guiLock");
                Program.KillFanCurveProcess();
                desktop.Shutdown();
            };
        }
        
        Closing += (s, e) =>
        {
           
            
            if (s is not Window window)
                return;

            
            
            if (window.IsVisible)
            {
                window.Hide();
                e.Cancel = true;
            }
            
            
        };

        Activated += async (s, e) =>
        {
            // await ViewModel!.CheckAndApplyAutoApplyProfile();
        };
        
        
        this.WhenActivated(action =>
        {
            
            action(ViewModel!.ShowOcProfileDialog.RegisterHandler(DoShowNewProfileDialogAsync));

            action(ViewModel!.ShowFanCurveEditorDialog.RegisterHandler(DoShowFanCurveEditorDialogAsync));
            
            action(ViewModel!.ShowSudoPasswordRequestDialog.RegisterHandler(DoShowSudoPasswordRequestDialogAsync));
            
            action(ViewModel!.ShowSettingsDialog.RegisterHandler(DoShowSettingsDialogAsync));
            action(ViewModel!.ShowUsageGraphsDialog.RegisterHandler(DoShowUsageGraphsDialogAsync));

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
        
        _defaultGuiGrid = MainOcWindow.Content as Grid;
        _defaultGuiResolution = [(int)MainOcWindow.Width, (int)MainOcWindow.Height];
        
        LoadGuiGrid(true);

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
    
    private async Task DoShowUsageGraphsDialogAsync(IInteractionContext<UsageGraphsWindowViewModel, object?> interaction)
    {
        var dialog = new UsageGraphsWindow
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<object?>(this);
        interaction.SetOutput(null);
    }
    
    public void LoadGuiGrid(bool firstRun = false)
    {
        if (Design.IsDesignMode)
        {
            Log.Information("Program executing in design mode. Skipping loading gui from file.");
            return;
        }
        
        
        
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
        
        
        // if (!File.Exists(Program.DefaultDataPath+"/SelectedGui.txt") || File.ReadAllText(Program.DefaultDataPath+"/SelectedGui.txt").Trim() == string.Empty)
        // {
        //     File.WriteAllText(Program.DefaultDataPath+"/SelectedGui.txt", "Default");
        // }
        
        //read selected gui name
        // var selectedGuiName =  (File.ReadAllText(Program.DefaultDataPath+"/SelectedGui.txt")).Trim();
        if (Program.LoadedSettings.SelectedGui == "Default")
        {
            if (firstRun)
            {
                Log.Information("Using default gui, skipping loading from file");
                return;
            }
            
            Log.Information("Restoring default gui.");

            if (_defaultGuiGrid is null || _defaultGuiResolution is null)
            {
                Log.Error("Error while applying default gui. Please restart the tool");
                return;
            }
            MainOcWindow.Width = _defaultGuiResolution[0];
            MainOcWindow.Height = _defaultGuiResolution[1];
            MainOcWindow.Content =  _defaultGuiGrid;
        }


        Stream? guiStream = null;
        int[] resolutionArr = [0, 0];
        
        //check if axaml file exists
        if (File.Exists(Program.DefaultDataPath + "/Guis/" + Program.LoadedSettings.SelectedGui+"/MainWindowMainGrid.axaml"))
        {
            Log.Information("GUI \""+Program.LoadedSettings.SelectedGui+"\" found in external path "+Program.DefaultDataPath + "/Guis/" + Program.LoadedSettings.SelectedGui+"/MainWindowMainGrid.axaml"+" . Loading...");
            
            if (File.Exists(Program.DefaultDataPath + "/Guis/" + Program.LoadedSettings.SelectedGui + "/MainWindowResolution.txt"))
            {
                try
                {
                    resolutionArr = File
                        .ReadAllText(Program.DefaultDataPath + "/Guis/" + Program.LoadedSettings.SelectedGui + "/MainWindowResolution.txt")
                        .Replace("*", "x").Trim().Split("x").Select(int.Parse).ToArray();

                    // Console.WriteLine("GUI " + selectedGuiName + " setting resolution: " + res[0] + "x" + res[1]);
                    
                    //MainOcWindow.MaxHeight = res[1];
                    //MainOcWindow.MinHeight = res[1];
                }
                catch (Exception ex)
                {
                    Log.Warning($"Exception while loading gui resolution, continuing with default: {ex}");
                }
            }
        
            var mainGridLoadPath = $"{Program.DefaultDataPath}/Guis/{Program.LoadedSettings.SelectedGui}/MainWindowMainGrid.axaml";
        
            guiStream = File.OpenRead(mainGridLoadPath);
            
        
            
        }

        if (guiStream is null)
        {
            var guisAssetFolderPath = new Uri("avares://ssprea-nvidia-control/Assets/MainWindowGuis/"+Program.LoadedSettings.SelectedGui);
            var guiMainWindowMainGridPath = new Uri(guisAssetFolderPath+ "/MainWindowMainGrid.customgui");    
            var guiResolutionPath = new Uri(guisAssetFolderPath+ "/MainWindowResolution.txt");

            if (!AssetLoader.Exists(guiMainWindowMainGridPath))
            {
                Log.Warning("Custom GUI not found even in assets, loading Default...");
                return;
            }
        
            Log.Information($"Found custom GUI in embedded assets at {guiMainWindowMainGridPath}, loading...");

            guiStream = AssetLoader.Open(guiMainWindowMainGridPath);
        
            if (AssetLoader.Exists(guiResolutionPath))
            {
                var streamReader = new StreamReader(AssetLoader.Open(guiResolutionPath));
                resolutionArr = streamReader.ReadToEnd().Replace("*", "x").Trim().Split("x").Select(int.Parse).ToArray();;
            }
        
            
        }
        //load from assets
        
        
        //load gui from stream
        if (guiStream is null)
        {
            Log.Error("An error occurred while loading the custom gui file, loading Default...");
            return;
        }
        
        var loadedGrid = AvaloniaRuntimeXamlLoader.Load(guiStream) as Grid;
        
        
        MainOcWindow.Content =  loadedGrid;
        
        //apply resolution
        Log.Information("GUI " + Program.LoadedSettings.SelectedGui + " setting resolution: " + resolutionArr[0] + "x" + resolutionArr[1]);
        
        MainOcWindow.Width = resolutionArr[0];
        MainOcWindow.Height = resolutionArr[1];
        
        Log.Information("Successfully loaded custom GUI!");
        
        //check if resolution file exists
        
        //Console.WriteLine("Successfully loaded Gui from: "+mainGridLoadPath);
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

    // private void FanComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    // {
    //     ViewModel.SelectedGpuFan = ((NvmlGpuFan)(e.AddedItems[0]));
    // }
    


    private void Window_OnClosed(object? sender, EventArgs e)
    {
        MainWindowViewModel.NvmlService.Shutdown();
    }

    
}