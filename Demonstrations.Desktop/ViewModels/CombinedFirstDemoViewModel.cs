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
        }

        public ChartDataBase FullChartData => _model.FullChartData;

        private ChartDataBase _currentData;
        public ChartDataBase CurrentData
        {
            get => _currentData;
            set => this.RaiseAndSetIfChanged(ref _currentData, value);
        }

        public double LowerX
        {
            get => _model.LowerX;
            set
            {
                _model.LowerX = value;

            }
        }

        public double UpperX
        {
            get => _model.UpperX;
            set
            {
                _model.UpperX = value;

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

        public bool ShowLabel => CurrentData?.Count <= 200;
        public bool HighlightPoints => CurrentData?.Count < 300;
        public double ChartThickness => CurrentData?.Count < 300 ? 2 : 1;

        public ReactiveCommand<Unit, Unit> GenerateCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleOverlayCommand { get; }
    }
}