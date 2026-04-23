using Avalonia.Media;
using Controls.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Demonstrations.Desktop.Models
{
    public class PieChartDemoModel : ReactiveObject
    {
        PieChartGenerator _generator;

        public PieChartDemoModel()
        {
            _generator = new PieChartGenerator();
            PieChart = _generator.GenerateDefaultData();

            SectorColors =
            [
                Brushes.DodgerBlue,
                Brushes.OrangeRed,
                Brushes.Gold,
                Brushes.MediumSeaGreen,
                Brushes.MediumPurple,
                Brushes.HotPink,
                Brushes.Teal,
                Brushes.Coral
            ];
        }

        private PieChartDataBase _pieChart;
        public PieChartDataBase PieChart
        {
            get => _pieChart;
            set => this.RaiseAndSetIfChanged(ref _pieChart, value);
        }

        private IList<IBrush> _sectorColors;
        public IList<IBrush> SectorColors
        {
            get => _sectorColors;
            set => this.RaiseAndSetIfChanged(ref _sectorColors, value);
        }

        public void Generate() => PieChart = _generator.GenerateRandomData(); 
}
}
