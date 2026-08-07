using System;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace ssprea_nvidia_control.Controls;

public class NonVirtualizingCarousel : Grid
{
    private Button? _backBtn;
    private Button? _nextBtn;


    
    #region StyledProperties
    
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<ResponsiveGridCarousel, int>(nameof(SelectedIndex), 0);

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set
        {
            SetValue(SelectedIndexProperty, value);
            OnSelectedIndexChanged(value);
        }
    }

    

    // public static readonly StyledProperty<Avalonia.Controls.Controls> ItemsProperty =
    //     AvaloniaProperty.Register<ResponsiveGridCarousel, Avalonia.Controls.Controls>(nameof(Items), new Avalonia.Controls.Controls() );
    //
    // public Avalonia.Controls.Controls Items
    // {
    //     get => GetValue(ItemsProperty);
    //     set => SetValue(ItemsProperty, value);
    // }
    
    
    
    
    #endregion

    // protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    // {
    //     base.OnApplyTemplate(e);
    //     
    //     _grid = e.NameScope.Get<Grid>("PART_MainGrid");
    //     _backBtn = e.NameScope.Get<Button>("PART_BackBtn");
    //     _nextBtn = e.NameScope.Get<Button>("PART_NextBtn");
    //
    //     if (_backBtn is null || _nextBtn is null || _grid is null)
    //     {
    //         throw new NullReferenceException($"either backBtn, nextBtn or MainGrid are null in the NonVirtualizingCarousel name: \"{Name}\"");
    //     }
    //     
    //     _backBtn.Click += OnBackBtnClicked;
    //     _nextBtn.Click += OnNextBtnClicked;
    //
    //     
    //     
    // }

    public NonVirtualizingCarousel()
    {
        _backBtn = new Button()
        {
            Name = "PART_PrevBtn",
            Content = "‹",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Left,
        };

        _nextBtn = new Button()
        {
            Name = "PART_NextBtn",
            Content = "›",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Right,
        };
        
        _backBtn.Click += OnBackBtnClicked;
        _nextBtn.Click += OnNextBtnClicked;
        
        Grid.SetColumn(_backBtn, 0);
        Grid.SetColumn(_nextBtn, 2);
        
        Children.AddRange([_backBtn, _nextBtn]);
        
        Children.CollectionChanged += ChildrenOnCollectionChanged;

    }
    
    private void ChildrenOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        
        
            Console.WriteLine("sburar");
        
        if (e?.NewItems is null) return;
        
        foreach (var c in e.NewItems)
        {
            Grid.SetColumn((Control)c, 1);
        }
    }

    public void ClearAllItemsExceptButtons()
    {
        Children.RemoveRange(2,Children.Count -2);
    }
    


    

    private void OnSelectedIndexChanged(int newValue)
    {
        //make children with index == newValue visible and the others hidden

        if (Children.Count < 4) return;
        
        for (int i = 2; i < Children.Count; i++)
        {
            Children[i].IsVisible = newValue == i;
        }
        
    }
    
    private void OnNextBtnClicked(object? sender, RoutedEventArgs e)
    {
        if (Children.Count <= 3) return;
        var idx = ( (SelectedIndex + 1) % Children.Count);
        SelectedIndex = idx == 0 ? 2 : idx;
    }

    private void OnBackBtnClicked(object? sender, RoutedEventArgs e)
    {
        if (Children.Count <= 3) return;
        var idx = ( (SelectedIndex - 1 + Children.Count) % Children.Count);
        SelectedIndex = idx == 1 ? Children.Count - 1 : idx;
        
    }
}