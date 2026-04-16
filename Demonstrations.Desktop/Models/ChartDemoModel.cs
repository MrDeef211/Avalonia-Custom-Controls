using Controls.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demonstrations.Desktop.Models
{
    public class ChartDemoModel : ReactiveObject
    {
        private ChartGenerator _chartGenerator;

        public ChartDemoModel()
        {
            _chartGenerator = new();
            ChartData = _chartGenerator.Generate();
        }

        private ChartDataBase _chartData;
        public ChartDataBase ChartData
        {
            get => _chartData;
            set => this.RaiseAndSetIfChanged(ref _chartData, value);
        }

        public void Generate() => ChartData = _chartGenerator.Generate();
    }
}
