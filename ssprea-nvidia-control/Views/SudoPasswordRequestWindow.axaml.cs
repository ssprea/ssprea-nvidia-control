using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DynamicData.Binding;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.ViewModels;
using ReactiveUI;

namespace ssprea_nvidia_control.Views;

public partial class SudoPasswordRequestWindow  : ReactiveWindow<SudoPasswordRequestWindowViewModel>
{
    public SudoPasswordRequestWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;

        PasswordBox.GotFocus += PasswordBoxGotFocusOrKeyDownHandler;
        PasswordBox.KeyDown += PasswordBoxGotFocusOrKeyDownHandler;
        
        
        // This line is needed to make the previewer happy (the previewer plugin cannot handle the following line).
        if (Design.IsDesignMode) return;
            
        this.WhenActivated(action => action(ViewModel!.SavePasswordCommand.Subscribe(Close)));
        
    }
    

    private void PasswordBoxGotFocusOrKeyDownHandler(object? sender, EventArgs e)
    {
        CapsLockWarningLabel.IsVisible = Utils.General.IsCapsLockEnabled();
        
        if (ViewModel is not null && e is KeyEventArgs keyArgs && keyArgs.Key == Key.Enter)
            ViewModel.ReturnIfPasswordCorrect();
        
    }

    private void WindowOpenedHandler(object? sender, EventArgs e)
    {
        PasswordBox.Focus();
    }
}