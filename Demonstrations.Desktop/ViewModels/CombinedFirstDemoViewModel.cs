using Avalonia.Media.Imaging;
using Common.Controls.Models;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Demonstrations.Desktop.ViewModels
{
    public class CombinedFirstDemoViewModel : PageViewModelBase
    {
        private readonly ChartGenerator _generator = new();

        public CombinedFirstDemoViewModel()
        {
            Title = "Совместная работа: График + Zoom + RangeSlider";
            GenerateNewData();

            // Команды
            GenerateCommand = ReactiveCommand.Create(GenerateNewData);
            ToggleOverlayCommand = ReactiveCommand.Create(() => { ShowOverlay = !ShowOverlay; });

            this.WhenAnyValue(x => x.LowerX, x => x.UpperX)
                .Subscribe(_ => ApplyFilter());

            this.WhenAnyValue(x => x.LowerX, x => x.UpperX)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(ShowLabel)));

            this.WhenAnyValue(x => x.LowerX, x => x.UpperX)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(ChartThickness)));

            this.WhenAnyValue(x => x.LowerX, x => x.UpperX)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(HighlightPoints)));

            this.WhenAnyValue(x => x.FullChartData)
                .Subscribe(_ => ApplyFilter());
        }

        private ChartDataBase _fullChartData;
        public ChartDataBase FullChartData
        {
            get => _fullChartData;
            set => this.RaiseAndSetIfChanged(ref _fullChartData, value);
        }

        private ChartDataBase _filteredChartData;
        public ChartDataBase FilteredChartData
        {
            get => _filteredChartData;
            private set => this.RaiseAndSetIfChanged(ref _filteredChartData, value);
        }

        private double _lowerX = -200;
        public double LowerX
        {
            get => _lowerX;
            set => this.RaiseAndSetIfChanged(ref _lowerX, value);
        }

        private double _upperX = 199;
        public double UpperX
        {
            get => _upperX;
            set => this.RaiseAndSetIfChanged(ref _upperX, value);
        }

        private bool _showOverlay = true;
        public bool ShowOverlay
        {
            get => _showOverlay;
            set => this.RaiseAndSetIfChanged(ref _showOverlay, value);
        }

        private ZoomState _zoomState = new();
        public ZoomState ZoomState
        {
            get => _zoomState;
            set => this.RaiseAndSetIfChanged(ref _zoomState, value);
        }

        public bool ShowLabel => (FilteredChartData.Count <= 200);

        public bool HighlightPoints => (FilteredChartData.Count < 300);

        public double ChartThickness => (FilteredChartData.Count < 300 ? 2 : 1);

        public ReactiveCommand<Unit, Unit> GenerateCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleOverlayCommand { get; }

        private void GenerateNewData()
        {
            FullChartData = _generator.Generate(-500, 500);
        }

        private void ApplyFilter()
        {
            if (FullChartData == null) return;

            var filtered = new ChartDataBase();
            foreach (var point in FullChartData.Chart)
            {
                if (point.Key >= LowerX && point.Key <= UpperX)
                    filtered.Add(point.Key, point.Value);
            }
            FilteredChartData = filtered;
        }
    }
}