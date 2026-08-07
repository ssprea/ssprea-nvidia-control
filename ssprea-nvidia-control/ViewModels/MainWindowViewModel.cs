using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.NVML;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using Serilog;
using SkiaSharp;
using ssprea_nvidia_control.Lang;
using ssprea_nvidia_control.Lang;
using ssprea_nvidia_control.Utils;
using Tmds.DBus.Protocol;


namespace ssprea_nvidia_control.ViewModels;

public partial class MainWindowViewModel : ViewModelBase  

{
    #region Interaction Definitions
    public Interaction<NewOcProfileWindowViewModel, OcProfile?> ShowOcProfileDialog { get; }
    public Interaction<FanCurveEditorWindowViewModel, FanCurveViewModel?> ShowFanCurveEditorDialog { get; }
    public Interaction<SudoPasswordRequestWindowViewModel, SudoPassword?> ShowSudoPasswordRequestDialog { get; }
    public Interaction<SettingsMainWindowViewModel, object?> ShowSettingsDialog { get; }
    public Interaction<UsageGraphsWindowViewModel, object?> ShowUsageGraphsDialog { get; }
    #endregion
    
    
    #region ICommand definitions
    public ICommand OpenNewProfileWindowCommand { get; private set; }
    public ReactiveCommand<FanCurveViewModel?,Unit> OpenFanCurveEditorCommand { get; private set; }
    public ICommand OpenSudoPasswordPromptCommand { get; private set; }
    public ICommand OpenSettingsWindowCommand { get; private set; }
    public ICommand OpenUsageGraphsWindowCommand { get; private set; }

    #endregion
    
    [ObservableProperty] private NvmlGpu? _selectedGpu;
    [ObservableProperty] private NvmlGpuFan? _selectedGpuFan;
    [ObservableProperty] private OcProfile? _selectedOcProfile;
    [ObservableProperty] private OcProfile? _selectedAutoApplyOcProfile;
    [ObservableProperty] private FanCurveViewModel? _selectedFanCurve;
    [ObservableProperty] private bool _isAutoApplyProfileChecked = false;
    [ObservableProperty] private OcProfile? _selectedStartupProfile;
    [ObservableProperty] private bool _isStartupProfileChecked = false;
    [ObservableProperty] private string _currentNvidiaDriverVersion = "Unknown";
    [ObservableProperty] private bool _isFanCurveIncludedInProfileChecked = true;
    [ObservableProperty] private uint _tunerCurrentCoreOffset = 0;
    [ObservableProperty] private uint _tunerCurrentMemoryOffset = 0;
    [ObservableProperty] private uint _tunerCurrentPowerLimitMw = 0;
    [ObservableProperty] private string _tunerCurrentProfileName = "";
    [ObservableProperty] private string _currentlyLoadedGuiName = "Default";
    [ObservableProperty] private string _selectedLocalizerLang = "it";
    [ObservableProperty] private ObservableCollection<string> _localizerLangs = new ObservableCollection<string>(["it","en"]);
    [ObservableProperty] private ObservableCollection<ObservablePoint> _selectedFanCurveGraphPoints = new();
    [ObservableProperty] private MaxSizeObservableCollection<ObservablePoint> _currentFanSpeedGraphPoints = new(1);
    [ObservableProperty] private bool _flashingAnimationRunning = false;
    
    //graph series
    [ObservableProperty] private ObservableCollection<ISeries> _fanCurveGraphSeries = new();
    
    private uint _selectedFanRadioButton = 0;
    private bool FanSpeedSliderVisible => _selectedFanRadioButton == 1;
    
    private readonly string _profilesServiceName = "snvctl-profile.service";
    
    
    //graph sync object
    public object GraphSyncObject { get; } = new object();
    
    //Axes styles for fan curve graph graph
    // [ObservableProperty] private SolidColorPaint _graphTooltipTextPaint = new SolidColorPaint(SKColors.Black) {SKTypeface = _fanCurveGraphTypeface};
    // private static readonly SKTypeface _fanCurveGraphTypeface = SKTypeface.FromFamilyName("Noto Sans Mono",SKFontStyleWeight.Normal,SKFontStyleWidth.Normal,SKFontStyleSlant.Upright);
    // private static readonly SKTypeface _fanCurveGraphTypeface =
    //     SKTypeface.FromStream(AssetLoader.Open(new Uri("avares://ssprea-nvidia-control/Assets/Fonts/NotoSans/NotoSans-Light.ttf")));
    
    public Axis[] FanCurveGraphXAxes { get; set; } =
        [
            new Axis
            {
                Name = "Temperature (°C)",
                NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) , 
                NameTextSize = 10,

                LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite), 
                TextSize = 10,
                

                SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 }  
            }
        ];

    
    
    public Axis[] FanCurveGraphYAxes { get; set; } =
        [
            new Axis
                {
                    Name = "Fan Speed (%)",
                    NamePaint = new SolidColorPaint(SKColors.AntiqueWhite) , 
                    NameTextSize = 10,

                    LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite)  , 
                    TextSize = 10,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) 
                    { 
                        StrokeThickness = 2, 
                        PathEffect = new DashEffect([ 3, 3 ]) 
                    } 
                }
        ];
    
    
    
    
    
    public uint TunerCurrentPowerLimitW {
        get => TunerCurrentPowerLimitMw / 1000;
        set
        {
            TunerCurrentPowerLimitMw = (uint)(value * 1000);

        }
            
    }

    

    partial void OnTunerCurrentPowerLimitMwChanged(uint oldValue, uint newValue)
    {
        OnPropertyChanged(nameof(TunerCurrentPowerLimitW));
    }

    private const string DEFAULT_SERVICE_DATA_PATH = "/etc/snvctl";

    //private ObservableCollection<ISeries> _fanCurveGraphSeries = new();

    private AutoResetEvent _sudoPasswordDialogClosed = new(false);

    
    private bool _autoApplyProfileLoaded = false;

    public MainWindowViewModel()
    {
        Lockfile.CheckAndUpdateLockfile();
        
        
        
        
        if (!Directory.Exists(Program.DefaultDataPath))
            Directory.CreateDirectory(Program.DefaultDataPath);
        
        if (!Directory.Exists(Program.DefaultDataPath+"/temp"))
            Directory.CreateDirectory(Program.DefaultDataPath+"/temp");
        
        foreach(var f in Directory.GetFiles(Program.DefaultDataPath+"/temp"))
            File.Delete(f);
        
        LoadFanCurvesFromFile();
        
        
            
        
        ShowOcProfileDialog = new Interaction<NewOcProfileWindowViewModel, OcProfile?>();
        OpenNewProfileWindowCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var ocProfileWindowViewModel = new NewOcProfileWindowViewModel(this);

            var result = await ShowOcProfileDialog.Handle(ocProfileWindowViewModel);
            
            if (result !=null)
                OcProfilesList.Add(result);

            await _profilesFileManager.UpdateProfilesFileAsync();
        });
        
        
        ShowFanCurveEditorDialog = new Interaction<FanCurveEditorWindowViewModel, FanCurveViewModel?>();
        OpenFanCurveEditorCommand = ReactiveCommand.CreateFromTask<FanCurveViewModel?>(async (toEdit) =>
        {
            var fanCurveEditorWindowViewModel = new FanCurveEditorWindowViewModel(toEdit);

            var result = await ShowFanCurveEditorDialog.Handle(fanCurveEditorWindowViewModel);

            if (result == null)
                return;
            
            if (FanCurvesList.Any(x => x.Name == result.Name))
            {
                //c'è già una curve con lo stesso nome, aggiorna quella
                FanCurveViewModel existingCurve = FanCurvesList.First(x => x.Name == result.Name);
                existingCurve.BaseFanCurve.CurvePoints = result.BaseFanCurve.CurvePoints;
            }
            else
            {
                //sennò aggiungila
                FanCurvesList.Add(result);
            }

            SelectedFanCurve = result;
            SelectedFanCurve?.UpdateSeries();
            
            OnPropertyChanged(nameof(SelectedFanCurve));
            await FanCurvesFileManager.SaveFanCurvesAsync(Program.DefaultDataPath+"/fan_curves.json", FanCurvesList.Select(x => x.BaseFanCurve));

            
            
            //UpdateProfilesFile("profiles.json");
        });
        
        ShowSudoPasswordRequestDialog = new Interaction<SudoPasswordRequestWindowViewModel, SudoPassword?>();
        OpenSudoPasswordPromptCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var sudoPasswordRequestWindowViewModel = new SudoPasswordRequestWindowViewModel();

            var result = await ShowSudoPasswordRequestDialog.Handle(sudoPasswordRequestWindowViewModel);



            if (result != null)
            {
                SudoPasswordManager.CurrentPassword = result;
            }
            _sudoPasswordDialogClosed.Set();

        });
        
        
        
        ShowSettingsDialog = new Interaction<SettingsMainWindowViewModel, object?>();
        OpenSettingsWindowCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var settingsWindowViewModel = new SettingsMainWindowViewModel();

            var result = await ShowSettingsDialog.Handle(settingsWindowViewModel);
            
            
            
        });
        
        ShowUsageGraphsDialog = new Interaction<UsageGraphsWindowViewModel, object?>();
        OpenUsageGraphsWindowCommand = ReactiveCommand.CreateFromTask<NvmlGpu>(async (targetGpu) =>
        {
            var usageGraphsViewModel = new UsageGraphsWindowViewModel(targetGpu);

            var result = await ShowUsageGraphsDialog.Handle(usageGraphsViewModel);
            
            
        });
        
        LoadOcProfileToTuner(new OcProfile("",0,0,SelectedGpu?.PowerLimitMinMw ?? 100000, (FanCurve?)null));

        FanCurveGraphSeries.Add(new LineSeries<ObservablePoint>(SelectedFanCurveGraphPoints)
        {
            GeometryStroke=new SolidColorPaint(SKColors.DodgerBlue) {StrokeThickness = 3},
            Stroke= new SolidColorPaint(SKColors.DodgerBlue) {StrokeThickness = 3},
            Fill = new SolidColorPaint(SKColors.DodgerBlue.WithAlpha(50)),
            YToolTipLabelFormatter = point => $"{point.Model?.Y}%",
            XToolTipLabelFormatter = point => $"Temp: {point.Model?.X}°C",
            LineSmoothness = 0
        });
        
        FanCurveGraphSeries.Add(new LineSeries<ObservablePoint>(CurrentFanSpeedGraphPoints)
        {
            GeometryStroke=new SolidColorPaint(SKColors.DarkRed) {StrokeThickness = 3},
            Stroke= new SolidColorPaint(SKColors.DarkRed) {StrokeThickness = 3},
            YToolTipLabelFormatter = point => $"{point.Model?.Y}%",
            XToolTipLabelFormatter = point => $"{Lang.Resources.TextCurrentTemp} {point.Model?.X}°C",
            LineSmoothness = 0
        });
        
        
    }

    
    
    partial void OnSelectedGpuChanged(NvmlGpu? value)
    {
        if (value is null || value.FansList.Count <= 0)
            return;
        
        value.FansList.First().PropertyChanged += (s, args) =>
        {
            if (args.PropertyName == "CurrentSpeed")
            {
                lock (GraphSyncObject)
                {
                    
                    if (SelectedFanCurve?.CurrentFanSpeedPoints.Count > 0 &&
                        ((int?)SelectedFanCurve?.CurrentFanSpeedPoints.First().X ?? 0) == value.GpuTemperature &&
                        ((int?)SelectedFanCurve?.CurrentFanSpeedPoints.First().Y ?? 0) == value.FansList[0].CurrentSpeed)
                        return;
                        
                    CurrentFanSpeedGraphPoints.Add(new ObservablePoint(value.GpuTemperature,value.FansList[0].CurrentSpeed));
                
                }
            }
        };
    }
    
    
    [RelayCommand]
    public async Task ShowComingSoonPopupAsync(string featureName)
    {
        await MessageBoxManager.GetMessageBoxStandard("Coming soon!", $"{featureName}: Coming Soon!",ButtonEnum.Ok,Icon.Forbidden).ShowAsync();
    }
    
    public void ResetTunerOptions()
    {
        LoadOcProfileToTuner(new OcProfile("",0,0,SelectedGpu?.PowerLimitMinMw ?? 100000, (FanCurve?)null));
    }

    public async Task DeleteSelectedFanProfile()
    {
        if (SelectedFanCurve is null)
        {
            await MessageBoxManager.GetMessageBoxStandard(Resources.MsgBoxTitleError,Resources.MsgBoxBodyNoFancurveSelected,ButtonEnum.Ok,Icon.Warning).ShowAsync();
            return;
        }

        var boxResult = await MessageBoxManager.GetMessageBoxStandard(Resources.MsgBoxTitleAreYouSure,
            $"{Resources.MsgBoxBodyAreYouSureDelete} \"{SelectedFanCurve.Name}\"?", ButtonEnum.YesNo,
            Icon.Question).ShowAsync();

        if (boxResult == ButtonResult.Yes)
        {
            FanCurvesList.Remove(SelectedFanCurve);
            if (FanCurvesList.Any())
                SelectedFanCurve = FanCurvesList.First();
            await FanCurvesFileManager.SaveFanCurvesAsync(Program.DefaultDataPath+"/fan_curves.json", FanCurvesList.Select(x => x.BaseFanCurve));
            
        }
        
        
    }

    public async Task SaveProfileAndUpdateFileAsync(OcProfile? profile)
    {
        if (profile != null)
        {
            if (!IsFanCurveIncludedInProfileChecked)
                profile.FanCurveName = "";
            if (OcProfilesList.Any(x => x.Name == profile.Name))
            {
                OcProfilesList.Remove(OcProfilesList.First(x => x.Name == profile.Name));
            }
            
            OcProfilesList.Add(profile);
        }
            
        
        await _profilesFileManager.UpdateProfilesFileAsync();
    }

    
    
    public async Task SaveTempTunerSettingsToProfileAndUpdateFileAsync()
    {
        if (string.IsNullOrEmpty(TunerCurrentProfileName) || string.IsNullOrWhiteSpace(TunerCurrentProfileName))
        {
            await MessageBoxManager.GetMessageBoxStandard(Resources.MsgBoxTitleWarning,Resources.MsgBoxBodyNewProfileMissingName,ButtonEnum.Ok,Icon.Warning).ShowAsync();
            return;
        }
        
        await SaveProfileAndUpdateFileAsync(new OcProfile(TunerCurrentProfileName, TunerCurrentCoreOffset,
            TunerCurrentMemoryOffset, TunerCurrentPowerLimitMw, SelectedFanCurve?.BaseFanCurve));
    }
    
    
    
    private void LoadOcProfileToTuner(OcProfile? ocProfile)
    {
        
        if (ocProfile == null)
            return;

        

        TunerCurrentCoreOffset = ocProfile.GpuClockOffset;
        TunerCurrentMemoryOffset = ocProfile.MemClockOffset;
        TunerCurrentPowerLimitMw = ocProfile.PowerLimitMw;
        TunerCurrentProfileName = ocProfile.Name;
        
        if (FanCurvesList.Any(x => x.Name == ocProfile.FanCurveName))
            SelectedFanCurve = FanCurvesList.First(x => x.Name == ocProfile.FanCurveName);


    }

    public async Task OnLoadProfileButtonClicked()
    {
        await LoadSelectedOcProfileToTuner();
        await RunFlashingAnimationForSeconds(2);
    }
    
    private async Task RunFlashingAnimationForSeconds(int seconds)
    {
        FlashingAnimationRunning = true;
        await Task.Delay(seconds * 1000);
        FlashingAnimationRunning = false;
    }
    
    public async Task LoadSelectedOcProfileToTuner()
    {
        if (SelectedOcProfile is null)
        {
            await MessageBoxManager.GetMessageBoxStandard(Resources.MsgBoxTitleError,Resources.MsgBoxBodyNoProfileSelected,ButtonEnum.Ok,Icon.Warning).ShowAsync();
            return;
        }
        
        LoadOcProfileToTuner(SelectedOcProfile);


    }

    
    
    
    
    private async Task CheckAndLoadStartupProfile()
    {
        
        
        //check startup profile
        IsStartupProfileChecked = Utils.Systemd.IsSystemdServiceEnabled(_profilesServiceName);
        if (IsStartupProfileChecked && File.Exists(DEFAULT_SERVICE_DATA_PATH+"/profile.json"))
        {
            var startupProfileName = OcProfile.FromJson(await File.ReadAllTextAsync(DEFAULT_SERVICE_DATA_PATH+"/profile.json"))?.Name;
            
            SelectedStartupProfile = OcProfilesList.FirstOrDefault(x => x.Name == startupProfileName);
            SelectedOcProfile = SelectedStartupProfile;
            await LoadSelectedOcProfileToTuner();
        }
        
        
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns>true if password success, false if cancel</returns>
    private async Task<bool> RequestSudoPasswordDialogIfNeededAsync()
    {
#if WINDOWS
        return true;
#else
        if (SudoPasswordManager.CurrentPassword is null)
        {
            OpenSudoPasswordPromptCommand.Execute(null);
            await Task.Run(() => _sudoPasswordDialogClosed.WaitOne());
        }
        return SudoPasswordManager.CurrentPassword is not null;
#endif
    }
    
    
    public async Task CheckAndApplyAutoApplyProfile()
    {
        //check default profile
        if (!_autoApplyProfileLoaded && File.Exists(Program.DefaultDataPath + "/AutoApplyProfile.json"))
        {
            var jobj = JObject.Parse(await File.ReadAllTextAsync(Program.DefaultDataPath + "/AutoApplyProfile.json"));
            var gpuid = (uint)jobj["gpu"];
            var profile = (string)jobj["profile"];
                
            //apply profile
            SelectedGpu = NvmlService.GpuList.FirstOrDefault(x => x.DeviceIndex == gpuid);
            SelectedOcProfile = OcProfilesList.FirstOrDefault(x => x.Name == profile);
            SelectedAutoApplyOcProfile = SelectedOcProfile;
            IsAutoApplyProfileChecked = true;
            
            await OcProfileApplyCommand();
            
            
        }  
    }
    
    partial void OnSelectedFanCurveChanged(FanCurveViewModel? value)
    {
        if (value is null)
            return;
        
        lock (GraphSyncObject)
        {
            SelectedFanCurveGraphPoints.Clear();
            SelectedFanCurveGraphPoints.AddRange(value.BaseFanCurve.CurvePoints.Select(x => new ObservablePoint(x.Temperature, x.FanSpeed)).ToArray());
            
        }
    }

    [RelayCommand]
    public async Task SaveAutoApplyProfile(OcProfile? profile)
    {
        //File.WriteAllText(Program.DefaultDataPath + "/AutoApplyProfile.json", JsonSerializer.Serialize(GpuProfilePairString));

        
        
        
        //show warning before applying

        var warnMsgResp = await MessageBoxManager.GetMessageBoxStandard("Warning!",
            "The default profile will be applied every time the GUI app is opened, NOT when the PC boots! Do not enable both together as it will cause a conflict. \n" +
            "If you use the startup profile, your profile will be loaded in the GUI automatically anyways, this option is for some special use cases.\n\n" +
            "If you are unsure, press \"No\" and enable the Startup profile instead of this.\n\n" +
            "Do you really want to save the default profile?",
            ButtonEnum.YesNo,Icon.Warning).ShowAsync();

        if (warnMsgResp == ButtonResult.No)
        {
            IsAutoApplyProfileChecked = false;
            return;
        }

        if (IsStartupProfileChecked)
        {
            await MessageBoxManager.GetMessageBoxStandard("Warning!",
                "You are already using the startup profile, if it works you don't need this option. \n" +
                "If you want to enable this, disable the startup profile first.",
                ButtonEnum.Ok,Icon.Warning).ShowAsync();
            
            IsAutoApplyProfileChecked = false;
            return;
        }
        
        if (!IsAutoApplyProfileChecked)
        {
            File.Delete(Program.DefaultDataPath + "/AutoApplyProfile.json");
            Log.Information("No default profile selected, disabled auto apply.");
            return;
        }
        
        if (profile is null)
        {
            MessageBoxManager.GetMessageBoxStandard("Warning", "No profile selected!", ButtonEnum.Ok, Icon.Warning);
            return;
        }
        
        if (SelectedGpu == null)
        {
            MessageBoxManager.GetMessageBoxStandard("Warning", "No gpu selected!", ButtonEnum.Ok, Icon.Warning);
            return;
        }
        
        await File.WriteAllTextAsync(Program.DefaultDataPath + "/AutoApplyProfile.json", $"{{\"profile\":\"{profile.Name}\",\"gpu\":\"{SelectedGpu.DeviceIndex}\"}}");
    }
    
    
    
    [RelayCommand]
    public async Task SaveStartupProfile(OcProfile? profile)
    {
        
        if (IsAutoApplyProfileChecked)
        {
            await MessageBoxManager.GetMessageBoxStandard(Resources.MsgBoxTitleWarning,
                Resources.MsgBoxBodyDefaultProfileEnabled,
                ButtonEnum.Ok,Icon.Warning).ShowAsync();
            IsStartupProfileChecked = false;
            return;

        }
        
        
        //check sudo password
        if (!await RequestSudoPasswordDialogIfNeededAsync())
            return;
        
        //check if profile service exists
        if (!Systemd.DoesSystemdServiceExist(_profilesServiceName))
        {
            await MessageBoxManager.GetMessageBoxStandard(Resources.MsgBoxTitleError,
                $"{_profilesServiceName} {Resources.MsgBoxBodyProfileServiceMissing}", ButtonEnum.Ok,
                Icon.Error).ShowAsync();
            IsStartupProfileChecked = false;
            return;
        }
        
        //if the checkbox is disabled, stop the service
        if (!IsStartupProfileChecked)
        {
            Systemd.StopSystemdService(_profilesServiceName);
            Systemd.DisableSystemdService(_profilesServiceName);
            
            Log.Information("No startup profile selected, stopped {serviceName}.", _profilesServiceName);
            SelectedStartupProfile = null;
            return;
        }

        if (profile is null)
        {
            MessageBoxManager.GetMessageBoxStandard(Resources.MsgBoxTitleWarning, Resources.MsgBoxBodyNoProfileSelected, ButtonEnum.Ok, Icon.Warning);
            IsStartupProfileChecked = false;
            
            return;
        }
        
        if (SelectedGpu == null)
        {
            MessageBoxManager.GetMessageBoxStandard(Resources.MsgBoxTitleWarning, Resources.MsgBoxBodyNoGpuSelected, ButtonEnum.Ok, Icon.Warning);
            IsStartupProfileChecked = false;
            
            return;
        }
        
        
        //check if directory exists
        if (!Directory.Exists(DEFAULT_SERVICE_DATA_PATH ))
            Files.MakeDirectorySudo(DEFAULT_SERVICE_DATA_PATH);

        
        //save profile and copy to service data path
        await File.WriteAllTextAsync(Program.DefaultDataPath + "/temp/deviceidx.txt", SelectedGpu.DeviceIndex.ToString());
        Files.CopySudo(Program.DefaultDataPath + "/temp/deviceidx.txt", DEFAULT_SERVICE_DATA_PATH+"/deviceidx.txt");
        
        await File.WriteAllTextAsync(Program.DefaultDataPath + "/temp/profile.json", profile.ToJson());
        Files.CopySudo(Program.DefaultDataPath + "/temp/profile.json", DEFAULT_SERVICE_DATA_PATH+"/profile.json");


        if (profile.FanCurve is not null)
        {
            //save fan curve and copy to service data path
            await File.WriteAllTextAsync(Program.DefaultDataPath + "/temp/curve.json", profile.FanCurve.ToJson());
            Files.CopySudo(Program.DefaultDataPath + "/temp/curve.json", DEFAULT_SERVICE_DATA_PATH+"/curve.json");

        }
        
        
        //enable service
        Systemd.EnableSystemdService(_profilesServiceName);
        if (Systemd.StartSystemdService(_profilesServiceName))
        {
            SelectedStartupProfile = SelectedOcProfile;
            //kill gui fan curve process if running
            Program.KillFanCurveProcess();
        }
    }
    
    //private readonly FanCurvesFileManager _fanCurvesFileManager = new("fan_curves.json");
    private readonly ProfilesFileManager _profilesFileManager=new(Program.DefaultDataPath+"/profiles.json");

    public ObservableCollection<OcProfile> OcProfilesList => _profilesFileManager.LoadedProfiles;


    public static ObservableCollection<FanCurveViewModel> FanCurvesList { get; private set; } = new();


    private void LoadFanCurvesFromFile()
    {
        foreach (var fanCurve in FanCurvesFileManager.GetFanCurves(Program.DefaultDataPath+"/fan_curves.json"))
        {
            FanCurvesList.Add(new FanCurveViewModel(fanCurve));
        }
    }

    public void KillFanCurveProcessCommand( )
    {
        Program.KillFanCurveProcess();
    }

    
    [RelayCommand]
    public void OpenDefaultBrowserToUrl(string destUrl)
    {
#if LINUX
        Process.Start(new ProcessStartInfo("xdg-open", destUrl));
#else
        Process.Start(destUrl);
#endif
    }
    

    public async Task DeleteOcProfile()
    {
        if (SelectedOcProfile is null)
        {
            await MessageBoxManager.GetMessageBoxStandard(Resources.MsgBoxTitleError, Resources.MsgBoxBodyNoProfileSelected, ButtonEnum.Ok, Icon.Warning).ShowAsync();
            return;
        }
        
        var boxResult = await MessageBoxManager.GetMessageBoxStandard(Resources.MsgBoxTitleAreYouSure, $"{Resources.MsgBoxBodyAreYouSureDelete} \"{SelectedOcProfile.Name}\"?", ButtonEnum.YesNo, Icon.Question).ShowAsync();

        if (boxResult == ButtonResult.Yes)
        {
            OcProfilesList.Remove(SelectedOcProfile);
            await _profilesFileManager.UpdateProfilesFileAsync();
        }
        
    }

    
    
    public async Task OcProfileApplyCommand()
    {
        await OcProfileParameterApplyCommand(SelectedOcProfile);
    }

    public async Task ApplyTempTunerSettings()
    {
        await OcProfileParameterApplyCommand(new OcProfile(TunerCurrentProfileName, TunerCurrentCoreOffset,
            TunerCurrentMemoryOffset, TunerCurrentPowerLimitMw, IsFanCurveIncludedInProfileChecked ? SelectedFanCurve?.BaseFanCurve : null));
    }
    
    private async Task OcProfileParameterApplyCommand(OcProfile? ocProfile)
    {
        if (SelectedGpu is null)
        {
            Log.Warning("No gpu selected!");
            return;
        }

        //check sudo password
        if (!await RequestSudoPasswordDialogIfNeededAsync())
            return;
        
        KillFanCurveProcessCommand();

        if (Utils.Systemd.IsSystemdServiceRunning(_profilesServiceName))
        {
            var box = MessageBoxManager.GetMessageBoxCustom(
                new MessageBoxCustomParams()
                {
                    ButtonDefinitions = new List<ButtonDefinition>
                    {
                        new ButtonDefinition { Name = "Cancel", IsDefault = true },
                        new ButtonDefinition { Name = "Apply and keep old fan profile" },
                        new ButtonDefinition { Name = "Stop service",  },
                    },
                    
                    ContentTitle = $"{_profilesServiceName} detected!",
                    ContentMessage = $"{_profilesServiceName} {Resources.MsgBoxBodyServiceConflict} 'sudo systemctl enable {_profilesServiceName}'",
                    Topmost = true,
                    CanResize = false,
                    Icon = Icon.Warning,
                    ShowInCenter = true,
                    WindowDecorations = WindowDecorations.BorderOnly
                }
            );

            var result = await box.ShowAsync();

            switch (result)
            {
                case "Stop service":
                    Utils.Systemd.StopSystemdService(_profilesServiceName);
                    IsStartupProfileChecked = false;
                    break;
                
                case "Apply and keep old fan profile":
                    break;
                
                default:
                    return;

            }
        }
        
        ocProfile?.Apply(SelectedGpu);
        _autoApplyProfileLoaded = true;
        
    }

    

    bool CanOcProfileApplyCommand()
    {
        return SelectedGpu != null;
    }
    
    public async Task<bool> FanApplyButtonClick(uint speed)
    {
        if (SelectedGpuFan is null || SelectedGpu is null) return false;

        //check sudo password
        if (!await RequestSudoPasswordDialogIfNeededAsync())
            return false;
        
        
        switch (_selectedFanRadioButton)
        {
            case 0:
                return SelectedGpu.ApplyAutoSpeedToAllFans();
            case 1:
                return SelectedGpu.ApplySpeedToAllFans(speed);
            default:
                return false;
        }
       
    }

    public void FanRadioButtonClicked(uint id)
    {
        //0: auto, 1: manual, 2:curve

        _selectedFanRadioButton = id;
        
    }

    public static NvmlService NvmlService { get; set; } = new();
    

    
    public void SelectGpu(uint id)
    {
        
    }

    public async Task LoadedEvent()
    {
        await ShowDependenciesMsgbox(await CheckDependencies());
        
        NvmlService.Initialize();
        
        
        if (SelectedGpu is null && NvmlService.GpuList.Any())
            SelectedGpu = NvmlService.GpuList.First();
        
        await CheckAndLoadStartupProfile();
        await CheckAndApplyAutoApplyProfile();
    }

    public void LoadProfileToTuner()
    {
        
    }

    /// <summary>
    /// Check nvidia drivers and cli tool
    /// </summary>
    /// <returns>0: success, 1: no nvidia driver, 2: nvidia driver version less than 555, 3: cli tool not installed </returns>
    private async Task<ushort> CheckDependencies()
    {
        //check nvidia drivers installed
# if LINUX
        var vercmd = Utils.General.RunCliCommand("nvidia-smi", "--version", true,false,true);
        if (vercmd is null || vercmd.ExitCode != 0)
            return 1;

        //check nvidia drivers version

        var output = await vercmd.StandardOutput.ReadToEndAsync();
        var lines = output.Split('\n');
        CurrentNvidiaDriverVersion = lines[2].Split(':')[1].Trim();

        if (CurrentNvidiaDriverVersion.StartsWith("Deprecated"))
        {
            CurrentNvidiaDriverVersion = lines[4].Split(':')[1].Trim();
        }

        Log.Information($"Detected NVidia driver version: {CurrentNvidiaDriverVersion}");
#endif
        //TODO: add windows driver check
        
        //check cli tool

        var clicmd = Utils.General.RunCliCommand("snvctl", "-d", true,false,true);
        if (clicmd is null || clicmd.ExitCode != 0)
            return 3;

        return 0;
    }

    public async Task ShowDependenciesMsgbox(ushort errCode)
    {
       
        
        switch (errCode)
        {
            case 0:
                return;
            case 1:

                var box = MessageBoxManager.GetMessageBoxStandard(Resources.MsgBoxTitleDependencyDriverMissing,
                    Resources.MsgBoxTitleDependencyDriverMissing, ButtonEnum.Ok, Icon.Error);

                await box.ShowAsync();
                Environment.Exit(1);
                break;
            case 2:
                box = MessageBoxManager.GetMessageBoxStandard(Resources.MsgBoxTitleDependencyDriverOutdated,
                    Resources.MsgBoxBodyDependencyDriverOutdated, ButtonEnum.Ok, Icon.Warning);

                await box.ShowAsync();
                break;
            case 3:
                box = MessageBoxManager.GetMessageBoxStandard(Resources.MsgBoxTitleDependencyCliMissing,
                    Resources.MsgBoxBodyDependencyCliMissing, ButtonEnum.Ok, Icon.Warning);

                await box.ShowAsync();
                break;
        }
    }
}
