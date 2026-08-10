using System;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Reactive;
using Serilog;

namespace ssprea_nvidia_control.Controls;

public class ResponsiveGridCarousel : TemplatedControl
{
    [Content] public Avalonia.Controls.Controls Children { get; } = new();

    #region StyledProperties

    

    
    public static readonly StyledProperty<ColumnDefinitions> GridColumnDefinitionsProperty =
        AvaloniaProperty.Register<ResponsiveGridCarousel, ColumnDefinitions>(nameof(GridColumnDefinitions), new ColumnDefinitions("*"));
    
    public ColumnDefinitions GridColumnDefinitions
    {
        get => GetValue(GridColumnDefinitionsProperty);
        set => SetValue(GridColumnDefinitionsProperty, value);
    }
    
    public static readonly StyledProperty<RowDefinitions> GridRowDefinitionsProperty =
        AvaloniaProperty.Register<ResponsiveGridCarousel, RowDefinitions>(nameof(GridRowDefinitions), new RowDefinitions("*"));
    
    public RowDefinitions GridRowDefinitions
    {
        get => GetValue(GridRowDefinitionsProperty);
        set => SetValue(GridRowDefinitionsProperty, value);
    }
    
    

    public static readonly StyledProperty<string> LabelTextProperty =
        AvaloniaProperty.Register<ResponsiveGridCarousel, string>(nameof(LabelText), "Default");
    
    public string LabelText
    {
        get => GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }
    
    public static readonly StyledProperty<double> BreakpointProperty =
        AvaloniaProperty.Register<ResponsiveGridCarousel, double>(nameof(Breakpoint), 600d);

    public double Breakpoint
    {
        get => GetValue(BreakpointProperty);
        set => SetValue(BreakpointProperty, value);
    }
    #endregion
    
    private Grid? _grid;
    private NonVirtualizingCarousel? _carousel;
    private bool _isWide = false;

    
    

    private void ApplyMode(bool wide, bool force = false)
    {
        if (_grid is null || _carousel is null ) return;
        if (!force && _isWide == wide) return;

        Log.Debug($"Applying mode {wide}");
        
        _isWide = wide;
        
        _grid.Children.Clear();
        _carousel.ClearAllItemsExceptButtons();
        
        
        
        
                
        
        //andare in modalità wide, grid diventa visible ,carousel diventa hidden e i children vengono aggiunti alla grid
        if (wide)
        {
            
            _grid.IsVisible = true;
            _carousel.IsVisible = false;
            

            for (int i = 0; i < Children.Count; i++)
            {
                var childControl = Children[i];
                
                if (childControl is CarousellableBorder border)
                    border.UnsetCarouselled();
                
                Grid.SetColumn(childControl, i);
                _grid.Children.Add(childControl);
                childControl.IsVisible = true;
            }
        }
        else
        {
            _grid.IsVisible = false;
            _carousel.IsVisible = true;
            foreach (var child in Children)
            {
                if (child is not null)
                {
                    if (child is CarousellableBorder border)
                        border.SetCarouselled();
                        
                    _carousel.Children.Add(child);
                }
            }

            _carousel.SelectedIndex = 2;
        }
          
         
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _grid = e.NameScope.Find<Grid>("PART_Grid");
        _carousel = e.NameScope.Find<NonVirtualizingCarousel>("PART_NVCarousel");


        //apply grid columns and rows definitions
        if (_grid is not null)
        {
            foreach (var def in GridRowDefinitions)
                _grid.RowDefinitions.Add(def);
            foreach (var def in GridColumnDefinitions)
                _grid.ColumnDefinitions.Add(def);
        }
        
        this.GetObservable(BoundsProperty).Subscribe((IObserver<Rect>)new AnonymousObserver<Rect>(r =>
            ApplyMode(r.Width >= Breakpoint)));
        
    }
    
   
    
    public ResponsiveGridCarousel()
    {
    }
}