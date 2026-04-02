using Avalonia.Controls;
using Avalonia.Media;
using Common.Controls;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;

namespace Demonstrations.Desktop.ViewModels
{
    internal class RangeSliderDemoViewModel : PageViewModelBase
    {
        public RangeSliderDemoViewModel()
        {
            Title = "Диапазон (RangeSlider)";

            // Генерация случайных значений
            RandomizeCommand = ReactiveCommand.Create(() =>
            {
                var rand = new Random();
                double min = rand.Next(0, 50);
                double max = min + rand.Next(20, 80);
                Minimum = min;
                Maximum = max;
                LowerValue = min + rand.NextDouble() * (max - min) * 0.3;
                UpperValue = max - rand.NextDouble() * (max - min) * 0.3;
                Step = rand.Next(1, 10);
            });

            this.ValidationRule(
                vm => vm.TickStepText,
                text => IsValidGridLength(text),
                "Неверный формат (Auto, * или число)");

            this.WhenAnyValue(x => x.TickStepText)
                .Subscribe(text =>
                {
                    if (IsValidGridLength(text))
                        TickStep = GridLength.Parse(text);
                });

            this.WhenAnyValue(x => x.LowerValue, x => x.UpperValue)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(Range)));
        }

        private double _minimum = 0;
        public double Minimum
        {
            get => _minimum;
            set => this.RaiseAndSetIfChanged(ref _minimum, value);
        }

        private double _maximum = 100;
        public double Maximum
        {
            get => _maximum;
            set => this.RaiseAndSetIfChanged(ref _maximum, value);
        }

        private double _lowerValue = 30;
        public double LowerValue
        {
            get => _lowerValue;
            set => this.RaiseAndSetIfChanged(ref _lowerValue, value);
        }

        private double _upperValue = 70;
        public double UpperValue
        {
            get => _upperValue;
            set => this.RaiseAndSetIfChanged(ref _upperValue, value);
        }

        private double _step = 1;
        public double Step
        {
            get => _step;
            set => this.RaiseAndSetIfChanged(ref _step, value);
        }

        private bool _showTextBoxes = true;
        public bool ShowTextBoxes
        {
            get => _showTextBoxes;
            set => this.RaiseAndSetIfChanged(ref _showTextBoxes, value);
        }

        private double _textBoxWidth = 60;
        public double TextBoxWidth
        {
            get => _textBoxWidth;
            set => this.RaiseAndSetIfChanged(ref _textBoxWidth, value);
        }

        private bool _isLowerThumbEnabled = true;
        public bool IsLowerThumbEnabled
        {
            get => _isLowerThumbEnabled;
            set => this.RaiseAndSetIfChanged(ref _isLowerThumbEnabled, value);
        }

        private bool _isUpperThumbEnabled = true;
        public bool IsUpperThumbEnabled
        {
            get => _isUpperThumbEnabled;
            set => this.RaiseAndSetIfChanged(ref _isUpperThumbEnabled, value);
        }

        private IBrush _lowerThumbBrush = Brushes.Gray;
        public IBrush LowerThumbBrush
        {
            get => _lowerThumbBrush;
            set => this.RaiseAndSetIfChanged(ref _lowerThumbBrush, value);
        }

        private IBrush _upperThumbBrush = Brushes.Gray;
        public IBrush UpperThumbBrush
        {
            get => _upperThumbBrush;
            set => this.RaiseAndSetIfChanged(ref _upperThumbBrush, value);
        }

        private IBrush _trackBrush = Brushes.LightGray;
        public IBrush TrackBrush
        {
            get => _trackBrush;
            set => this.RaiseAndSetIfChanged(ref _trackBrush, value);
        }

        private FillMode _fillMode = FillMode.Between;
        public FillMode FillMode
        {
            get => _fillMode;
            set => this.RaiseAndSetIfChanged(ref _fillMode, value);
        }

        public IEnumerable<FillMode> AllFillModes => Enum.GetValues(typeof(FillMode)).Cast<FillMode>();

        private IBrush _fillBrush = Brushes.DodgerBlue;
        public IBrush FillBrush
        {
            get => _fillBrush;
            set => this.RaiseAndSetIfChanged(ref _fillBrush, value);
        }

        private bool _showEndStops = true;
        public bool ShowEndStops
        {
            get => _showEndStops;
            set => this.RaiseAndSetIfChanged(ref _showEndStops, value);
        }

        private string _tickStepText = "Auto";
        public string TickStepText
        {
            get => _tickStepText;
            set => this.RaiseAndSetIfChanged(ref _tickStepText, value);
        }

        private GridLength _tickStep = GridLength.Auto;
        public GridLength TickStep
        {
            get => _tickStep;
            private set => this.RaiseAndSetIfChanged(ref _tickStep, value);
        }

        private IBrush _tickBrush = Brushes.Black;
        public IBrush TickBrush
        {
            get => _tickBrush;
            set => this.RaiseAndSetIfChanged(ref _tickBrush, value);
        }

        private double _tickLength = 5;
        public double TickLength
        {
            get => _tickLength;
            set => this.RaiseAndSetIfChanged(ref _tickLength, value);
        }

        public double Range => UpperValue - LowerValue;

        public ReactiveCommand<Unit, Unit> RandomizeCommand { get; }

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
