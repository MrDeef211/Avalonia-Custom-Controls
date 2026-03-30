using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.TextFormatting;
using Common.Controls;
using Common.Controls.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using ReactiveUI.Validation;
using ReactiveUI.Validation.Extensions;
using Avalonia.Controls.Converters;

namespace Demonstrations.Desktop.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel() 
        {
            var canExecuteTest = this.WhenAnyValue(x => x.TestCommandIsActive);
            ChartData = new();

            // Комманды

            ClickCommand = ReactiveCommand.Create(() =>
            {
                TestCommandIsActive = !TestCommandIsActive;
            });

            TestCommand = ReactiveCommand.Create(() => { }, canExecuteTest);

            DrawCommand = ReactiveCommand.Create(() =>
            {
                var dB = new ChartDataBase();
                var rand = new Random();
                dB.Add(-200, 0);
                for (int i = -199; i < 200; i++)
                {
                    var dy = rand.Next(0, 100);
                    dB.Add(i, dB.Chart[i-1] + Math.Cos(dy) * 100 + ( - dB.Chart[i - 1]) / 10 * Math.Abs(Math.Cos(dy)));
                }
                ChartData = dB;
            }, canExecuteTest);

            // Валидация

            this.ValidationRule(
                vm => vm.GridSizeXText,
                input => IsValidGridLength(input),
                "Неверный формат \n(Auto, * или число)");

            this.ValidationRule(
                vm => vm.GridSizeYText,
                input => IsValidGridLength(input),
                "Неверный формат \n(Auto, * или число)");

            // Привязки событий

            this.WhenAnyValue(x => x.GridSizeXText)
                .Subscribe(text =>
                {
                    if (IsValidGridLength(text))
                    {
                        GridSizeX = GridLength.Parse(text);
                    }
                });

            this.WhenAnyValue(x => x.GridSizeYText)
                .Subscribe(text =>
                {
                    if (IsValidGridLength(text))
                    {
                        GridSizeY = GridLength.Parse(text);
                    }
                });


        }

        private bool _testCommandIsActive = true;
        public bool TestCommandIsActive
        {
            get => _testCommandIsActive;
            set => this.RaiseAndSetIfChanged(ref _testCommandIsActive, value);
        }

        private bool _isFill;
        public bool IsFill
        {
            get => _isFill;
            set => this.RaiseAndSetIfChanged(ref _isFill, value);
        }

        private bool _grid = true;
        public bool Grid
        {
            get => _grid;
            set => this.RaiseAndSetIfChanged(ref _grid, value);
        }

        private string _gridSizeXText = "Auto";
        public string GridSizeXText 
        {
            get => _gridSizeXText;
            set => this.RaiseAndSetIfChanged(ref _gridSizeXText, value);
        }

        private GridLength _gridSizeX;
        public GridLength GridSizeX
        {
            get => _gridSizeX;
            private set => this.RaiseAndSetIfChanged(ref _gridSizeX, value);
        }

        private string _gridSizeYText = "Auto";
        public string GridSizeYText 
        {
            get => _gridSizeYText;
            set => this.RaiseAndSetIfChanged(ref _gridSizeYText, value);
        }

        private GridLength _gridSizeY;
        public GridLength GridSizeY
        {
            get => _gridSizeY;
            private set => this.RaiseAndSetIfChanged(ref _gridSizeY, value);
        }

        private ChartDataBase  _chartData;
        public ChartDataBase ChartData
        {
            get => _chartData;
            set => this.RaiseAndSetIfChanged(ref _chartData, value);
        }

        public IEnumerable<ChartStyle> AllChartStyles =>
            Enum.GetValues(typeof(ChartStyle)).Cast<ChartStyle>();

        private ChartStyle _selectedChartStyle = ChartStyle.Line;
        public ChartStyle SelectedChartStyle
        {
            get => _selectedChartStyle;
            set => this.RaiseAndSetIfChanged(ref _selectedChartStyle, value);
        }

        private bool _highlightPoints = true;
        public bool HighlightPoints
        {
            get => _highlightPoints;
            set => this.RaiseAndSetIfChanged(ref _highlightPoints, value);
        }

        private bool _showLabels = true;
        public bool ShowLabels
        {
            get => _showLabels;
            set => this.RaiseAndSetIfChanged(ref _showLabels, value);
        }

        public IEnumerable<AxisLabelMode> AllLabelModes =>
            Enum.GetValues(typeof(AxisLabelMode)).Cast<AxisLabelMode>();

        private AxisLabelMode _labelModeX = AxisLabelMode.Auto;
        public AxisLabelMode LabelModeX
        {
            get => _labelModeX;
            set => this.RaiseAndSetIfChanged(ref _labelModeX, value);
        }

        private AxisLabelMode _labelModeY = AxisLabelMode.Auto;
        public AxisLabelMode LabelModeY
        {
            get => _labelModeY;
            set => this.RaiseAndSetIfChanged(ref _labelModeY, value);
        }

        private bool IsValidGridLength(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            try
            {
                var result = GridLength.Parse(input);
                return result is GridLength;
            }
            catch
            {
                return false;
            }
        }

        public ReactiveCommand<Unit,Unit> ClickCommand { get; }

        public ReactiveCommand<Unit, Unit> TestCommand { get; }

        public ReactiveCommand<Unit, Unit> DrawCommand { get; }
    }
}
