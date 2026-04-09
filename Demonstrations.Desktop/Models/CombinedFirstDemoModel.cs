using Common.Controls.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demonstrations.Desktop.Models
{
    public class CombinedFirstDemoModel : ReactiveObject
    {
        private readonly ChartGenerator _generator = new();

        public CombinedFirstDemoModel()
        {
            GenerateNewData();


            this.WhenAnyValue(x => x.FullChartData)
                .Subscribe(_ => ApplyFilter());

            this.WhenAnyValue(x => x.LowerX, x => x.UpperX)
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

        public void GenerateNewData()
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
