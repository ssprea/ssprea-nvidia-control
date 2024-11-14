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
using CommunityToolkit.Mvvm.ComponentModel;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.NVML;
using ssprea_nvidia_control.NVML.NvmlTypes;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI;


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
    private readonly ProfilesFileManager _profilesFileManager=new("profiles.json");

    public ObservableCollection<OcProfile> OcProfilesList => _profilesFileManager.LoadedProfiles;


    public static ObservableCollection<FanCurveViewModel> FanCurvesList { get; private set; } = new();


    private void LoadFanCurvesFromFile()
    {
        foreach (var fanCurve in FanCurvesFileManager.GetFanCurves("fan_curves.json"))
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

    private uint _selectedFanRadioButton = 0;

    private bool FanSpeedSliderVisible => _selectedFanRadioButton == 1;
    
    public ICommand OpenNewProfileWindowCommand { get; private set; }
    public ICommand OpenFanCurveEditorCommand { get; private set; }

    
    
    



    public MainWindowViewModel()
    {
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
            
            
            await FanCurvesFileManager.SaveFanCurvesAsync("fan_curves.json", FanCurvesList.Select(x => x.BaseFanCurve));
            
                

            //UpdateProfilesFile("profiles.json");
        });
    }


    

    public void DeleteOcProfile()
    {
        if (SelectedOcProfile is not null)
            OcProfilesList.Remove(SelectedOcProfile);
        FanCurvesFileManager.SaveFanCurves("fan_curves.json", FanCurvesList.Select(x => x.BaseFanCurve));
    }
    
    public void OcProfileApplyCommand()
    {
        if (SelectedGpu is null)
        {
            Console.WriteLine("No gpu selected!");
            return;
        }
        
        
        
        SelectedOcProfile?.Apply(SelectedGpu);
    }

    bool CanOcProfileApplyCommand()
    {
        return SelectedGpu != null;
    }
    
    public bool FanApplyButtonClick(uint speed)
    {
        if (SelectedGpuFan == null) return false;

        
        switch (_selectedFanRadioButton)
        {
            case 0:
                return SelectedGpuFan.SetPolicy(NvmlFanControlPolicy.NVML_FAN_POLICY_TEMPERATURE_CONTINOUS_SW);
            case 1:
                return SelectedGpuFan.SetSpeed(speed);
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
