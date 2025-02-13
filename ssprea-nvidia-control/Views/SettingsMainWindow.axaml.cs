using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ssprea_nvidia_control.Models;
using ssprea_nvidia_control.ViewModels;
using ReactiveUI;


namespace ssprea_nvidia_control.Views;

public partial class SettingsMainWindow : ReactiveWindow<SettingsMainWindowViewModel>
{
    public SettingsMainWindow()
    {
        InitializeComponent();
        
        // This line is needed to make the previewer happy (the previewer plugin cannot handle the following line).
        if (Design.IsDesignMode) return;
            
        this.WhenActivated(action => action(ViewModel!.CloseSettingsCommand.Subscribe(Close)));
    }
}