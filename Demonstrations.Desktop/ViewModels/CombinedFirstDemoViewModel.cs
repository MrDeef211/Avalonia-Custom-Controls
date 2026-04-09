using Avalonia.Input;
using Avalonia.Media.Imaging;
using Common.Controls.Models;
using Demonstrations.Desktop.Models;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Demonstrations.Desktop.ViewModels
{
    public class CombinedFirstDemoViewModel : PageViewModelBase
    {

        private CombinedFirstDemoModel _model = new();

        public CombinedFirstDemoViewModel()
        {
            Title = "Совместная работа: График + Zoom + RangeSlider";

            // Команды
            GenerateCommand = ReactiveCommand.Create(() => _model.GenerateNewData());
            ToggleOverlayCommand = ReactiveCommand.Create(() => { ShowOverlay = !ShowOverlay; });

            this.WhenAnyValue(x => x._model.LowerX, x => x._model.UpperX)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(ShowLabel));
                    this.RaisePropertyChanged(nameof(ChartThickness));
                    this.RaisePropertyChanged(nameof(HighlightPoints));
                });

            this.WhenAnyValue(x => x._model.FilteredChartData)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(FilteredChartData)));
        }

        public ChartDataBase FilteredChartData => _model.FilteredChartData;

        public double LowerX
        {
            get => _model.LowerX;
            set
            {
                _model.LowerX = value;
                this.RaisePropertyChanged();
            }
        }

        public double UpperX
        {
            get => _model.UpperX;
            set
            {
                _model.UpperX = value;
                this.RaisePropertyChanged();
            }
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
    }
}