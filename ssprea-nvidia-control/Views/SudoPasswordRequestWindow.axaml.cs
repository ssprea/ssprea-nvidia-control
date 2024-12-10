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
        // This line is needed to make the previewer happy (the previewer plugin cannot handle the following line).
        if (Design.IsDesignMode) return;
            
        this.WhenActivated(action => action(ViewModel!.SavePasswordCommand.Subscribe(Close)));
        
        // PasswordBox.WhenValueChanged(textBox => textBox.Text).Subscribe(text =>
        // {
        //     if (PasswordBox.Text != "")
        //         PasswordBox.Text = "";
        //     if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
        //         return;
        //     
        //     Console.WriteLine("textchanged: "+text);
        //     ViewModel!.CurrentSudoPassword.Password.AppendChar(char.Parse(text));
        // });
    }


    

    
}