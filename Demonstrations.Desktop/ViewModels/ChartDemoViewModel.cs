using Avalonia.Controls;
using Controls;
using Controls.Models;
using Demonstrations.Desktop.Models;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;

namespace Demonstrations.Desktop.ViewModels
{
    internal class ChartDemoViewModel : PageViewModelBase
    {
        private ChartDemoModel _model;
        public ChartDemoViewModel()
        {
            Title = "График";   
            
            _model = new ChartDemoModel();

            // Команды
            DrawCommand = ReactiveCommand.Create(() => _model.Generate(), this.WhenAnyValue(x => x.TestCommandIsActive));

            // Валидация
            this.ValidationRule(
                vm => vm.GridSizeXText,
                input => IsValidGridLength(input),
                "Неверный формат \n(Auto, * или число)");

            this.ValidationRule(
                vm => vm.GridSizeYText,
                input => IsValidGridLength(input),
                "Неверный формат \n(Auto, * или число)");

            // Привязки
            this.WhenAnyValue(x => x.GridSizeXText)
                .Subscribe(text =>
                {
                    if (IsValidGridLength(text))
                        GridSizeX = GridLength.Parse(text);
                });

            this.WhenAnyValue(x => x.GridSizeYText)
                .Subscribe(text =>
                {
                    if (IsValidGridLength(text))
                        GridSizeY = GridLength.Parse(text);
                });

            this.WhenAnyValue(x => x._model.ChartData)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(ChartData)));
        }

        public ChartDataBase ChartData => _model.ChartData;

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

        private GridLength _gridSizeX = GridLength.Auto;
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

        private GridLength _gridSizeY = GridLength.Auto;
        public GridLength GridSizeY
        {
            get => _gridSizeY;
            private set => this.RaiseAndSetIfChanged(ref _gridSizeY, value);
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

        private bool _showLabels = false;
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

        // Команды
        public ReactiveCommand<Unit, Unit> DrawCommand { get; }

        private bool IsValidGridLength(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            try
            {
                GridLength.Parse(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

