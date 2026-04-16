using Controls.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demonstrations.Desktop.Models
{
    public class CombinedFirstDemoModel : ReactiveObject
    {
        private readonly ChartGenerator _generator = new();

        public CombinedFirstDemoModel()
        {
            GenerateNewData();
        }

        private ChartDataBase _fullChartData;
        public ChartDataBase FullChartData
        {
            get => _fullChartData;
            set => this.RaiseAndSetIfChanged(ref _fullChartData, value);
        }

        private double _lowerX;
        public double LowerX
        {
            get => _lowerX;
            set => this.RaiseAndSetIfChanged(ref _lowerX, value);
        }

        private double _upperX;
        public double UpperX
        {
            get => _upperX;
            set => this.RaiseAndSetIfChanged(ref _upperX, value);
        }

        public void GenerateNewData()
        {
            FullChartData = _generator.Generate(-1000, 1000);
            var keys = FullChartData.Chart.Keys.OrderBy(k => k).ToList();
            LowerX = keys.First();
            UpperX = keys.Last();
        }
    }
}
