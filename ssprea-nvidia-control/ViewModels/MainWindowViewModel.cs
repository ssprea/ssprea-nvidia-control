﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.NVML;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using ssprea_nvidia_control.Models.Exceptions;


namespace ssprea_nvidia_control.ViewModels;

public partial class MainWindowViewModel : ViewModelBase  

{
    [ObservableProperty] private NvmlGpu? _selectedGpu;
    [ObservableProperty] private NvmlGpuFan? _selectedGpuFan;
    [ObservableProperty] private OcProfile? _selectedOcProfile;
    [ObservableProperty] private OcProfile? _selectedAutoApplyOcProfile;
    [ObservableProperty] private FanCurveViewModel? _selectedFanCurve;
    [ObservableProperty] private bool _isAutoApplyProfileChecked = false;
    [ObservableProperty] private OcProfile? _selectedStartupProfile;
    [ObservableProperty] private bool _isStartupProfileChecked = false;

    private const string DEFAULT_SERVICE_DATA_PATH = "/etc/snvctl";

    //private ObservableCollection<ISeries> _fanCurveGraphSeries = new();

    private AutoResetEvent _sudoPasswordDialogClosed = new(false);

    
    private bool _autoApplyProfileLoaded = false;

    public async Task CheckAndLoadStartupProfile()
    {
        
        
        //check startup profile
        IsStartupProfileChecked = Utils.Systemd.IsSystemdServiceRunning("snvctl.service");
        if (IsStartupProfileChecked && File.Exists(DEFAULT_SERVICE_DATA_PATH+"/profile.json"))
        {
            var startupProfileName = OcProfile.FromJson(await File.ReadAllTextAsync(DEFAULT_SERVICE_DATA_PATH+"/profile.json"))?.Name;
            
            SelectedStartupProfile = OcProfilesList.FirstOrDefault(x => x.Name == startupProfileName);
        }
        
        
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns>true if password success, false if cancel</returns>
    private async Task<bool> RequestSudoPasswordDialogIfNeededAsync()
    {
        if (SudoPasswordManager.CurrentPassword is null)
        {
            OpenSudoPasswordPromptCommand.Execute(null);
            await Task.Run(() => _sudoPasswordDialogClosed.WaitOne());
        }
        return SudoPasswordManager.CurrentPassword is not null;
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

        if (!IsAutoApplyProfileChecked || profile is null)
        {
            File.Delete(Program.DefaultDataPath + "/AutoApplyProfile.json");
            Console.WriteLine("No default profile selected, disabled auto apply.");
            return;
        }
        
        if (SelectedGpu == null)
        {
            Console.WriteLine("No gpu selected.");
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
        if (!IsStartupProfileChecked || profile is null)
        {
            Utils.Systemd.StopSystemdService("snvctl.service");
            
            Console.WriteLine("No startup profile selected, stopped snvctl.service");
            return;
        }
        
        if (SelectedGpu == null)
        {
            Console.WriteLine("No gpu selected.");
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
After=power-profiles-daemon.service
[Service]
Type=simple
ExecStart=/bin/bash -c 'snvctl --forceOpen -g {SelectedGpu.DeviceIndex} -op {DEFAULT_SERVICE_DATA_PATH}/profile.json -fp {DEFAULT_SERVICE_DATA_PATH}/curve.json'  &
[Install]
WantedBy=default.target";

        //write to temp file and copy to service data path
        File.WriteAllText(Program.DefaultDataPath + "/temp/snvctl.service", service);
        Utils.Files.CopySudo(Program.DefaultDataPath + "/temp/snvctl.service", "/etc/systemd/system/snvctl.service");
        
        //enable service
        Utils.Systemd.RunSystemdCommand("daemon-reload");
        Utils.Systemd.EnableSystemdService("snvctl.service");
        Utils.Systemd.StartSystemdService("snvctl.service");

        

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
    
    
    // public ObservableCollection<FanCurveViewModel> FanCurvesVMList
    // {
    //     get
    //     {
    //         var fanCurves = new ObservableCollection<FanCurveViewModel>();
    //         foreach (var fanCurve in FanCurvesList)
    //         {
    //             fanCurves.Add(new FanCurveViewModel(fanCurve));
    //         }
    //         return fanCurves;
    //     }
    // }


    
    
    public Interaction<NewOcProfileWindowViewModel, OcProfile?> ShowOcProfileDialog { get; }
    public Interaction<FanCurveEditorWindowViewModel, FanCurveViewModel?> ShowFanCurveEditorDialog { get; }
    public Interaction<SudoPasswordRequestWindowViewModel, SudoPassword?> ShowSudoPasswordRequestDialog { get; }

    private uint _selectedFanRadioButton = 0;
    

    private bool FanSpeedSliderVisible => _selectedFanRadioButton == 1;
    
    public ICommand OpenNewProfileWindowCommand { get; private set; }
    public ICommand OpenFanCurveEditorCommand { get; private set; }
    public ICommand OpenSudoPasswordPromptCommand { get; private set; }


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
        OpenFanCurveEditorCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var fanCurveEditorWindowViewModel = new FanCurveEditorWindowViewModel();

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
        
        
          
    }

    

    public void DeleteOcProfile()
    {
        if (SelectedOcProfile is not null)
            OcProfilesList.Remove(SelectedOcProfile);
        FanCurvesFileManager.SaveFanCurves(Program.DefaultDataPath+"/fan_curves.json", FanCurvesList.Select(x => x.BaseFanCurve));
    }
    
    public async Task OcProfileApplyCommand()
    {
        if (SelectedGpu is null)
        {
            Console.WriteLine("No gpu selected!");
            return;
        }

        //check sudo password
        if (!await RequestSudoPasswordDialogIfNeededAsync())
            return;
        
        // trycommand:
        // try
        // {
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
                                     "NOTE: if you decide to stop the service, you will have to re-enable the startup profile or run 'sudo systemctl start snvctl.service'",
                    Topmost = true,
                    CanResize = false,
                    Icon = Icon.Warning,
                    ShowInCenter = true,
                    SystemDecorations = SystemDecorations.BorderOnly
                }
            );

            var result = await box.ShowAsync();
            Console.WriteLine("msgbox result: "+result);

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
        
        SelectedOcProfile?.Apply(SelectedGpu);
        _autoApplyProfileLoaded = true;
        // }catch (SudoPasswordExpiredException)
        // {
        //     //sudo password expired, reprompt
        //     if (await RequestSudoPasswordDialogIfNeededAsync());
        //     goto trycommand;
        //
        // }
    }

    // public void RetryApplyIfPasswordRequested()
    // {
    //     tryApply:
    //     try
    //     {
    //         OcProfileApplyCommand();
    //         return;
    //     }
    //     catch (SudoPasswordExpiredException)
    //     {
    //         goto tryApply;
    //     }
    // }
    
    // public void OcProfileApplyCommand(NvmlGpu? gpu, OcProfile? profile)
    // {
    //     if (gpu is null)
    //     {
    //         Console.WriteLine("No gpu selected!");
    //         return;
    //     }
    //
    //
    //     try
    //     {
    //         profile?.Apply(gpu);
    //     }catch (SudoPasswordExpiredException)
    //     {
    //         //sudo password expired, reprompt
    //         OpenSudoPasswordPromptCommand.Execute(null);
    //     }
    // }
    

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
    

}
