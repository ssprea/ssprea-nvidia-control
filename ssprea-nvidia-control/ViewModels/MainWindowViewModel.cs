using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text.Json;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.NVML;
using ssprea_nvidia_control.NVML.NvmlTypes;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI;
using ssprea_nvidia_control.Models.Exceptions;


namespace ssprea_nvidia_control.ViewModels;

public partial class MainWindowViewModel : ViewModelBase  

{
    [ObservableProperty] private NvmlGpu? _selectedGpu;
    [ObservableProperty] private NvmlGpuFan? _selectedGpuFan;
    [ObservableProperty] private OcProfile? _selectedOcProfile;
    [ObservableProperty] private FanCurveViewModel? _selectedFanCurve;

    //private ObservableCollection<ISeries> _fanCurveGraphSeries = new();
    
        
   
    
    partial void OnSelectedFanCurveChanged(FanCurveViewModel? value)
    {
        //Console.WriteLine(SelectedFanCurve.Name);
        SelectedFanCurve.UpdateSeries();
        // _fanCurveGraphSeries.Clear();
        // _fanCurveGraphSeries.Add(SelectedFanCurve.CurvePointsSeries);
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
            
            if (result !=null)
                SudoPasswordManager.CurrentPassword = result;

        });
    }


    

    public void DeleteOcProfile()
    {
        if (SelectedOcProfile is not null)
            OcProfilesList.Remove(SelectedOcProfile);
        FanCurvesFileManager.SaveFanCurves(Program.DefaultDataPath+"/fan_curves.json", FanCurvesList.Select(x => x.BaseFanCurve));
    }
    
    public void OcProfileApplyCommand()
    {
        if (SelectedGpu is null)
        {
            Console.WriteLine("No gpu selected!");
            return;
        }


        try
        {
            SelectedOcProfile?.Apply(SelectedGpu);
        }catch (SudoPasswordExpiredException)
        {
            //sudo password expired, reprompt
            OpenSudoPasswordPromptCommand.Execute(null);
        }
    }

    bool CanOcProfileApplyCommand()
    {
        return SelectedGpu != null;
    }
    
    public bool FanApplyButtonClick(uint speed)
    {
        if (SelectedGpuFan is null || SelectedGpu is null) return false;

        try
        {
            switch (_selectedFanRadioButton)
            {
                case 0:
                    return SelectedGpu.ApplyAutoSpeedToAllFans();
                case 1:
                    return SelectedGpu.ApplySpeedToAllFans(speed);
                default:
                    return false;
            }
        }catch (SudoPasswordExpiredException)
        {
            //sudo password expired, reprompt
            OpenSudoPasswordPromptCommand.Execute(null);
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
