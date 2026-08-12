using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using sspreaNvidiaControl.Models;
using ReactiveUI;
using sspreaNvidiaControl.ViewModels;

namespace sspreaNvidiaControl.Views;

public partial class FanCurveEditorWindow : ReactiveWindow<FanCurveEditorWindowViewModel>
{
    public FanCurveEditorWindow()
    {
        InitializeComponent();
        
        // This line is needed to make the previewer happy (the previewer plugin cannot handle the following line).
        if (Design.IsDesignMode) return;
            
        this.WhenActivated(action => action(ViewModel!.SaveCurveCommand.Subscribe(Close)));
    }


    
}