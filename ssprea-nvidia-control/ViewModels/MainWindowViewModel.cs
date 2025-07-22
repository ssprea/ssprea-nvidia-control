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
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
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
using SkiaSharp;
using ssprea_nvidia_control.Models.Exceptions;


namespace ssprea_nvidia_control.ViewModels;

public partial class MainWindowViewModel : ViewModelBase  

{
    #region Interaction Definitions
    public Interaction<NewOcProfileWindowViewModel, OcProfile?> ShowOcProfileDialog { get; }
    public Interaction<FanCurveEditorWindowViewModel, FanCurveViewModel?> ShowFanCurveEditorDialog { get; }
    public Interaction<SudoPasswordRequestWindowViewModel, SudoPassword?> ShowSudoPasswordRequestDialog { get; }
    public Interaction<SettingsMainWindowViewModel, object?> ShowSettingsDialog { get; }
    #endregion
    
    
    #region ICommand definitions
    public ICommand OpenNewProfileWindowCommand { get; private set; }
    public ReactiveCommand<FanCurveViewModel?,Unit> OpenFanCurveEditorCommand { get; private set; }
    public ICommand OpenSudoPasswordPromptCommand { get; private set; }
    public ICommand OpenSettingsWindowCommand { get; private set; }

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
    
    private uint _selectedFanRadioButton = 0;
    private bool FanSpeedSliderVisible => _selectedFanRadioButton == 1;
    
    
    //Axes styles for fan curve graph graph
    public Axis[] FanCurveGraphXAxes { get; set; } =
        [
            new Axis
            {
                Name = "Temperature",
                NamePaint = new SolidColorPaint(SKColors.AntiqueWhite), 
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
                    Name = "Fan Speed",
                    NamePaint = new SolidColorPaint(SKColors.AntiqueWhite), 
                    NameTextSize = 10,

                    LabelsPaint = new SolidColorPaint(SKColors.AntiqueWhite), 
                    TextSize = 10,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) 
                    { 
                        StrokeThickness = 2, 
                        PathEffect = new DashEffect([ 3, 3 ]) 
                    } 
                }
        ];
    
    
    public uint TunerCurrentPowerLimitW => TunerCurrentPowerLimitMw / 1000;

    

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
        
        LoadOcProfileToTuner(new OcProfile("",0,0,SelectedGpu?.PowerLimitMinMw ?? 100000, (FanCurve?)null));
    }

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
            await MessageBoxManager.GetMessageBoxStandard("Error","No fan profile selected!",ButtonEnum.Ok,Icon.Warning).ShowAsync();
            return;
        }

        var boxResult = await MessageBoxManager.GetMessageBoxStandard("Are you sure?",
            $"Are you sure you want to delete fan profile \"{SelectedFanCurve.Name}\"?", ButtonEnum.YesNo,
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
            await MessageBoxManager.GetMessageBoxStandard("Warning","Plase input a name for the new profile",ButtonEnum.Ok,Icon.Warning).ShowAsync();
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
    
    public async Task LoadSelectedOcProfileToTuner()
    {
        if (SelectedOcProfile is null)
        {
            await MessageBoxManager.GetMessageBoxStandard("Error","No profile selected!",ButtonEnum.Ok,Icon.Warning).ShowAsync();
            return;
        }
        
        LoadOcProfileToTuner(SelectedOcProfile);
    }

    
    
    private async Task CheckAndLoadStartupProfile()
    {
        
        
        //check startup profile
        IsStartupProfileChecked = Utils.Systemd.IsSystemdServiceRunning("snvctl.service");
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
        //Console.WriteLine(SelectedFanCurve.Name);
        SelectedFanCurve!.UpdateSeries();
        // _fanCurveGraphSeries.Clear();
        // _fanCurveGraphSeries.Add(SelectedFanCurve.CurvePointsSeries);
    }

    public void SaveAutoApplyProfile(OcProfile? profile)
    {
        //File.WriteAllText(Program.DefaultDataPath + "/AutoApplyProfile.json", JsonSerializer.Serialize(GpuProfilePairString));

        if (!IsAutoApplyProfileChecked)
        {
            File.Delete(Program.DefaultDataPath + "/AutoApplyProfile.json");
            Console.WriteLine("No default profile selected, disabled auto apply.");
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
        
        File.WriteAllText(Program.DefaultDataPath + "/AutoApplyProfile.json", $"{{\"profile\":\"{profile.Name}\",\"gpu\":\"{SelectedGpu.DeviceIndex}\"}}");
    }
    
    
    
    
    public async Task SaveStartupProfile(OcProfile? profile)
    {
        //check sudo password
        if (!await RequestSudoPasswordDialogIfNeededAsync())
            return;
        
        //if the checkbox is disabled, stop the service
        if (!IsStartupProfileChecked)
        {
            Utils.Systemd.StopSystemdService("snvctl.service");
            Utils.Systemd.DisableSystemdService("snvctl.service");
            
            Console.WriteLine("No startup profile selected, stopped snvctl.service");
            SelectedStartupProfile = null;
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
        
        
        
        //check if directory exists
        if (!Directory.Exists(DEFAULT_SERVICE_DATA_PATH ))
            Utils.Files.MakeDirectorySudo(DEFAULT_SERVICE_DATA_PATH);

        
        //save profile and copy to service data path
        await File.WriteAllTextAsync(Program.DefaultDataPath + "/temp/profile.json", profile.ToJson());
        Utils.Files.CopySudo(Program.DefaultDataPath + "/temp/profile.json", DEFAULT_SERVICE_DATA_PATH+"/profile.json");


        if (profile.FanCurve is not null)
        {
            //save fan curve and copy to service data path
            await File.WriteAllTextAsync(Program.DefaultDataPath + "/temp/curve.json", profile.FanCurve.ToJson());
            Utils.Files.CopySudo(Program.DefaultDataPath + "/temp/curve.json", DEFAULT_SERVICE_DATA_PATH+"/curve.json");

        }
        
        
        
        //systemd service (thanks to @Joomsy)
        string service = $@"
[Unit]
Description=Set the Nvidia GPU power profile

[Service]
Type=exec
ExecStart=/usr/local/bin/snvctl --forceOpen -g {SelectedGpu.DeviceIndex} -op {DEFAULT_SERVICE_DATA_PATH}/profile.json -fp {DEFAULT_SERVICE_DATA_PATH}/curve.json

[Install]
WantedBy=multi-user.target
";

        //write to temp file and copy to service data path
        await File.WriteAllTextAsync(Program.DefaultDataPath + "/temp/snvctl.service", service);
        Utils.Files.CopySudo(Program.DefaultDataPath + "/temp/snvctl.service", "/etc/systemd/system/snvctl.service");
        
        //enable service
        Utils.Systemd.RunSystemdCommand("daemon-reload");
        Utils.Systemd.EnableSystemdService("snvctl.service");
        if (Utils.Systemd.StartSystemdService("snvctl.service"))
            SelectedStartupProfile = SelectedOcProfile;
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
            await MessageBoxManager.GetMessageBoxStandard("Error", "No profile selected!", ButtonEnum.Ok, Icon.Warning).ShowAsync();
            return;
        }
        
        var boxResult = await MessageBoxManager.GetMessageBoxStandard("Are you sure?", $"Are you sure you want to delete profile \"{SelectedOcProfile.Name}\"?", ButtonEnum.YesNo, Icon.Question).ShowAsync();

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
            Console.WriteLine("No gpu selected!");
            return;
        }

        //check sudo password
        if (!await RequestSudoPasswordDialogIfNeededAsync())
            return;
        
        KillFanCurveProcessCommand();

        if (Utils.Systemd.IsSystemdServiceRunning("snvctl.service"))
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
                    
                    ContentTitle = "snvctl.service detected!",
                    ContentMessage = "snvctl.service (startup profile) is currently active, applying a fan profile with another instance already running can cause problems. \n" +
                                     "WARNING: if you decide to stop the service, you will have to re-enable the startup profile or run 'sudo systemctl start snvctl.service'",
                    Topmost = true,
                    CanResize = false,
                    Icon = Icon.Warning,
                    ShowInCenter = true,
                    SystemDecorations = SystemDecorations.BorderOnly
                }
            );

            var result = await box.ShowAsync();

            switch (result)
            {
                case "Stop service":
                    Utils.Systemd.StopSystemdService("snvctl.service");
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
        
        await CheckAndLoadStartupProfile();
        
        if (SelectedGpu is null && NvmlService.GpuList.Any())
            SelectedGpu = NvmlService.GpuList.First(); 
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

        Console.WriteLine($"Detected NVidia driver version: {CurrentNvidiaDriverVersion}");
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

                var box = MessageBoxManager.GetMessageBoxStandard("Nvidia driver not detected!",
                    "Please make sure you installed Nvidia proprietary driver version 555+", ButtonEnum.Ok, Icon.Error);

                await box.ShowAsync();
                Environment.Exit(1);
                break;
            case 2:
                box = MessageBoxManager.GetMessageBoxStandard("Nvidia driver version outdated!",
                    "The nvidia driver was detected but to apply settings version 555+ is required", ButtonEnum.Ok, Icon.Warning);

                await box.ShowAsync();
                break;
            case 3:
                box = MessageBoxManager.GetMessageBoxStandard("snvctl cli tool not detected!",
                    "Please make sure you installed the CLI tool otherwise you won't be able to apply settings.", ButtonEnum.Ok, Icon.Warning);

                await box.ShowAsync();
                break;
        }
    }
}
