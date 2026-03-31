using Avalonia.Controls;
using Avalonia.Media;
using Common.Controls;
using Common.Controls.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;

namespace Demonstrations.Desktop.ViewModels
{
    public class PieChartDemoViewModel : PageViewModelBase
    {
        public PieChartDemoViewModel()
        {
            Title = "Круговая диаграмма";

            Content = new PieChartDataBase();
            GenerateDefaultData();

            SectorColors = new ObservableCollection<IBrush>
            {
                Brushes.DodgerBlue,
                Brushes.OrangeRed,
                Brushes.Gold,
                Brushes.MediumSeaGreen,
                Brushes.MediumPurple,
                Brushes.HotPink,
                Brushes.Teal,
                Brushes.Coral
            };

            HoleColor = Brushes.White;

            GenerateRandomDataCommand = ReactiveCommand.Create(GenerateRandomData);
            ResetDataCommand = ReactiveCommand.Create(ResetData);
            PickHoleColorCommand = ReactiveCommand.Create(PickHoleColor);
        }

        private PieChartDataBase _content;
        public PieChartDataBase Content
        {
            get => _content;
            set => this.RaiseAndSetIfChanged(ref _content, value);
        }

        private IList<IBrush> _sectorColors;
        public IList<IBrush> SectorColors
        {
            get => _sectorColors;
            set => this.RaiseAndSetIfChanged(ref _sectorColors, value);
        }

        private bool _highlightSector = true;
        public bool HighlightSector
        {
            get => _highlightSector;
            set => this.RaiseAndSetIfChanged(ref _highlightSector, value);
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

        private IImage _centerImage;
        public IImage CenterImage
        {
            get => _centerImage;
            set => this.RaiseAndSetIfChanged(ref _centerImage, value);
        }

        // Команды
        public ReactiveCommand<Unit, Unit> GenerateRandomDataCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetDataCommand { get; }
        public ReactiveCommand<Unit, Unit> PickHoleColorCommand { get; }

        private void GenerateDefaultData()
        {
            Content = new PieChartDataBase();
            Content.Add("Категория A", 35);
            Content.Add("Категория B", 25);
            Content.Add("Категория C", 20);
            Content.Add("Категория D", 15);
            Content.Add("Категория E", 5);
        }

        private void GenerateRandomData()
        {
            var rand = new Random();
            var newData = new PieChartDataBase();
            int sectorsCount = rand.Next(3, 8);
            for (int i = 0; i < sectorsCount; i++)
            {
                newData.Add($"Сектор {i + 1}", rand.Next(1, 50));
            }
            Content = newData;
        }

        private void ResetData()
        {
            GenerateDefaultData();
        }

        private async void PickHoleColor()
        {
            var colorDialog = new ColorPicker();
            HoleColor = Brushes.LightGray;
        }
    }
}

