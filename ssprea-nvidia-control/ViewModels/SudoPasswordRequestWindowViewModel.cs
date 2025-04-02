using System;
using System.Reactive;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData.Kernel;
using ReactiveUI;
using ssprea_nvidia_control.Models;

namespace ssprea_nvidia_control.ViewModels;

public partial class SudoPasswordRequestWindowViewModel : ViewModelBase
{
    //[ObservableProperty] private SudoPassword? _currentSudoPassword;
    public ReactiveCommand<Unit, SudoPassword> SavePasswordCommand { get; }

    [ObservableProperty] private string _passwordBoxText = "";
    private bool IsPasswordCorrect = false;

    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _isOperationCanceled = false;

    // partial void OnPasswordBoxTextChanging(string? oldValue, string newValue)
    // {
    //     //scrivi in textbox
    //     //per ogni lettera scritta prendi il valore di textbox.text
    //     //aggiungi alla securestring
    //     //azzera textbox.text
    //
    //     Console.WriteLine($"oldvalue: {oldValue} \t newValue: {newValue}");
    //     if (string.IsNullOrEmpty(newValue) || string.IsNullOrWhiteSpace(newValue))
    //         return;
    //     
    //     Console.WriteLine("LETTERA PSW: "+newValue);
    //     
    // }
    //
    // partial void OnPasswordBoxTextChanged(string? oldValue, string newValue)
    // {
    //     //Task.Delay(100).GetAwaiter().GetResult();
    //     
    //     if (string.IsNullOrEmpty(newValue) || string.IsNullOrWhiteSpace(newValue))
    //         return;
    //     if (PasswordBoxText.Length >= 2)
    //         PasswordBoxText = "";
    //     
    // }
    
    
    public SudoPasswordRequestWindowViewModel()
    {
        SavePasswordCommand = ReactiveCommand.Create(() => new SudoPassword(PasswordBoxText){OperationCanceled = IsOperationCanceled});
    }

    public void CloseDialogCommand()
    {
        IsOperationCanceled = true;
        SavePasswordCommand.Execute().Subscribe();
    }

    public void ReturnIfPasswordCorrect()
    {
        ErrorMessage="Checking password";
        var psw = new SudoPassword(PasswordBoxText);
        IsPasswordCorrect = psw.IsValid;
        
        if (IsPasswordCorrect)
            SavePasswordCommand.Execute().Subscribe();
        else
        {
            ErrorMessage="Incorrect password";
        }
        
    }
}