using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Controls;
using Controls.Models;
using Demonstrations.Desktop.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demonstrations.Desktop.ViewModels
{
    public class PieChartDemoViewModel : PageViewModelBase
    {

        private PieChartDemoModel _model;

        public PieChartDemoViewModel()
        {
            Title = "Круговая диаграмма";

            _model = new();

            HoleColor = Brushes.WhiteSmoke;

            // Команды
            GenerateRandomDataCommand = ReactiveCommand.Create(() => _model.Generate());
            PickCenterImageCommand = ReactiveCommand.CreateFromTask(PickCenterImageAsync);

            // Взаимодействия
            ShowOpenFileDialog = new Interaction<Unit, string?>();

            _model.WhenAnyValue(x => x.PieChart)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(Content)));

            _model.WhenAnyValue(x => x.SectorColors)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(SectorColors)));
        }

        public PieChartDataBase Content => _model.PieChart;

        public IList<IBrush> SectorColors => _model.SectorColors;

        private bool _highlightSector = true;
        public bool HighlightSector
        {
            get => _highlightSector;
            set => this.RaiseAndSetIfChanged(ref _highlightSector, value);
        }

        public static IEnumerable<HighlightType> AllHighlightType =>
            Enum.GetValues<HighlightType>().Cast<HighlightType>();
        
        private HighlightType _highlightType = HighlightType.Push;
        public HighlightType HighlightType
        {
            get => _highlightType;
            set => this.RaiseAndSetIfChanged(ref _highlightType, value);
        }

        private bool _showLabels = true;
        public bool ShowLabels
        {
            get => _showLabels;
            set => this.RaiseAndSetIfChanged(ref _showLabels, value);
        }

        private bool _showPercentages = true;
        public bool ShowPercentages
        {
            get => _showPercentages;
            set => this.RaiseAndSetIfChanged(ref _showPercentages, value);
        }

        public static IEnumerable<LabelPlacement> AllLabelPlacements =>
            Enum.GetValues<LabelPlacement>().Cast<LabelPlacement>();

        private LabelPlacement _labelPlacement = LabelPlacement.Outside;
        public LabelPlacement LabelPlacement
        {
            get => _labelPlacement;
            set => this.RaiseAndSetIfChanged(ref _labelPlacement, value);
        }

        private double _innerRadius = 0;
        public double InnerRadius
        {
            get => _innerRadius;
            set => this.RaiseAndSetIfChanged(ref _innerRadius, value);
        }

        private double _startAngle = 0;
        public double StartAngle
        {
            get => _startAngle;
            set => this.RaiseAndSetIfChanged(ref _startAngle, value);
        }

        private IBrush _holeColor;
        public IBrush HoleColor
        {
            get => _holeColor;
            set => this.RaiseAndSetIfChanged(ref _holeColor, value);
        }

        private IImage? _centerImage;
        public IImage? CenterImage
        {
            get => _centerImage;
            set => this.RaiseAndSetIfChanged(ref _centerImage, value);
        }


        private double _imageZoom = 1;
        public double ImageZoom
        {
            get => _imageZoom;
            set => this.RaiseAndSetIfChanged(ref _imageZoom, value);
        }

        public static IEnumerable<ImageScaling> AllImageScaling =>
            Enum.GetValues<ImageScaling>().Cast<ImageScaling>();

        private ImageScaling _imageScaling;
        public ImageScaling ImageScaling
        {
            get => _imageScaling;
            set => this.RaiseAndSetIfChanged(ref _imageScaling, value);
        }

        // Команды
        public ReactiveCommand<Unit, Unit> GenerateRandomDataCommand { get; }
        public ReactiveCommand<Unit, Unit> PickHoleColorCommand { get; }
        public ReactiveCommand<Unit, Unit> PickCenterImageCommand { get; }

        // Взаимодействия для диалогов
        public Interaction<Unit, string?> ShowOpenFileDialog { get; }

        private async Task PickCenterImageAsync()
        {
            var path = await ShowOpenFileDialog.Handle(Unit.Default);
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    var bitmap = await Task.Run(() => new Bitmap(path));
                    CenterImage = bitmap;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки: {ex.Message}");
                }
            }
        }
    }
}

