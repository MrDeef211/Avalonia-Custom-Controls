using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Controls.Models
{
    public class SectorData
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public double StartAngle { get; set; }
        public double SweepAngle { get; set; }
        public IBrush Brush { get; set; }
        public double Percentage => Value / Total * 100;
        public double Total { get; set; }
    }
}
