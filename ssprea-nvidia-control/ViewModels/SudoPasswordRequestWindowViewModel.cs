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
    public ReactiveCommand<Unit, SudoPassword?> SavePasswordCommand { get; }
    private SudoPassword? _sudoPassword;
    private bool _isPasswordCorrect = false;

    [ObservableProperty] private string _passwordBoxText = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _isErrorVisible = false;
    [ObservableProperty] private bool _isCapsLockWarningVisible = false;
    
    
    public SudoPasswordRequestWindowViewModel() 
    {
        SavePasswordCommand = ReactiveCommand.Create(() => _sudoPassword);
    }

    public void CloseDialogCommand()
    {
        SavePasswordCommand.Execute().Subscribe();
    }

    public void ReturnIfPasswordCorrect()
    {
        IsErrorVisible = false;
        var psw = new SudoPassword(PasswordBoxText);
        _isPasswordCorrect = psw.IsValid;

        if (_isPasswordCorrect)
        {
            _sudoPassword = psw;
            SavePasswordCommand.Execute().Subscribe();
            
        }
        else
        {
            IsErrorVisible = true;
        }
        
    }
}