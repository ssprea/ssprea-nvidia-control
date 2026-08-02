using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using Avalonia.Threading;

namespace ssprea_nvidia_control.Controls;

public class ResponsiveGrid : ItemsControl
{
    public static readonly StyledProperty<double> BreakpointProperty =
            AvaloniaProperty.Register<ResponsiveGrid, double>(nameof(Breakpoint), 600d);

        public double Breakpoint
        {
            get => GetValue(BreakpointProperty);
            set => SetValue(BreakpointProperty, value);
        }

        private ResponsivePanel? _panel;
        private Button? _prevButton;
        private Button? _nextButton;
        private IDisposable? _isWideSubscription;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (_prevButton != null) _prevButton.Click -= OnPrevClick;
            if (_nextButton != null) _nextButton.Click -= OnNextClick;

            _prevButton = e.NameScope.Find<Button>("PART_PrevButton");
            _nextButton = e.NameScope.Find<Button>("PART_NextButton");

            if (_prevButton != null) _prevButton.Click += OnPrevClick;
            if (_nextButton != null) _nextButton.Click += OnNextClick;

            // il panel viene realizzato dall'ItemsPresenter dopo l'applicazione del template
            // quindi lo agganciamo al passaggio di layout successivo.
            Dispatcher.UIThread.Post(HookPanel, DispatcherPriority.Loaded);
        }

        private void HookPanel()
        {
            _isWideSubscription?.Dispose();

            if (ItemsPanelRoot is ResponsivePanel panel)
            {
                _panel = panel;
                panel.Breakpoint = Breakpoint;

                _isWideSubscription = panel.GetObservable(ResponsivePanel.IsWideProperty).Subscribe((IObserver<bool>)new AnonymousObserver<bool>(isWide =>
                {
                    PseudoClasses.Set(":wide", isWide);
                    PseudoClasses.Set(":narrow", !isWide);
                }));
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == BreakpointProperty && _panel != null)
                _panel.Breakpoint = Breakpoint;
        }

        private void OnPrevClick(object? sender, RoutedEventArgs e) => _panel?.Previous();
        private void OnNextClick(object? sender, RoutedEventArgs e) => _panel?.Next();
   
}