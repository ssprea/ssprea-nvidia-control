using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace ssprea_nvidia_control.Controls;

public class ResponsivePanel : Panel
{
    public static readonly StyledProperty<double> BreakpointProperty =
            AvaloniaProperty.Register<ResponsivePanel, double>(nameof(Breakpoint), 600d);

        public double Breakpoint
        {
            get => GetValue(BreakpointProperty);
            set => SetValue(BreakpointProperty, value);
        }

        public static readonly StyledProperty<int> SelectedIndexProperty =
            AvaloniaProperty.Register<ResponsivePanel, int>(nameof(SelectedIndex));

        public int SelectedIndex
        {
            get => GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public static readonly DirectProperty<ResponsivePanel, bool> IsWideProperty =
            AvaloniaProperty.RegisterDirect<ResponsivePanel, bool>(nameof(IsWide), o => o.IsWide);

        private bool _isWide = true;
        public bool IsWide
        {
            get => _isWide;
            private set => SetAndRaise(IsWideProperty, ref _isWide, value);
        }

        static ResponsivePanel()
        {
            AffectsArrange<ResponsivePanel>(SelectedIndexProperty);
            AffectsMeasure<ResponsivePanel>(BreakpointProperty);
        }

        private Size UpdateContent(Size availableSize)
        {
            if (Children.Count == 0)
                return default;

            var width = double.IsInfinity(availableSize.Width) ? Breakpoint : availableSize.Width;
            IsWide = width >= Breakpoint;

            double desiredHeight = 0;

            if (IsWide)
            {
                var columnWidth = width / Children.Count;
                foreach (var child in Children)
                {
                    child.IsVisible = true;
                    child.Measure(new Size(columnWidth, availableSize.Height));
                    desiredHeight = Math.Max(desiredHeight, child.DesiredSize.Height);
                }
            }
            else
            {
                var index = Math.Clamp(SelectedIndex, 0, Children.Count - 1);
                for (int i = 0; i < Children.Count; i++)
                {
                    if (i == index)
                    {
                        Children[i].IsVisible = true;
                        Children[i].Measure(availableSize);
                        desiredHeight = Math.Max(desiredHeight, Children[i].DesiredSize.Height);
                    }
                    else
                    {
                        Children[i].Measure(default); 
                        Children[i].IsVisible = false;
                    }
                }
            }

            return new Size(width, double.IsInfinity(availableSize.Height) ? desiredHeight : availableSize.Height);
        }
        
        protected override Size MeasureOverride(Size availableSize)
        {
            return UpdateContent(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 0)
                return finalSize;

            if (IsWide)
            {
                var columnWidth = finalSize.Width / Children.Count;
                for (int i = 0; i < Children.Count; i++)
                    Children[i].Arrange(new Rect(i * columnWidth, 0, columnWidth, finalSize.Height));
            }
            else
            {
                var index = Math.Clamp(SelectedIndex, 0, Children.Count - 1);
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].Arrange(i == index
                        ? new Rect(0, 0, finalSize.Width, finalSize.Height)
                        : default);
                }
            }

            return finalSize;
        }

        public void Next()
        {
            if (Children.Count == 0) return;
            SelectedIndex = (SelectedIndex + 1) % Children.Count;
            UpdateContent(this.Bounds.Size);
        }

        public void Previous()
        {
            if (Children.Count == 0) return;
            SelectedIndex = (SelectedIndex - 1 + Children.Count) % Children.Count;
            UpdateContent(this.Bounds.Size);
        }
    
}