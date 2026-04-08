using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Controls.Models
{
    public class ZoomState : ReactiveObject
    {
        private double _scale = 1.0;
        private double _offsetX = 0;
        private double _offsetY = 0;

        public double Scale
        {
            get => _scale;
            set => this.RaiseAndSetIfChanged(ref _scale, value);
        }

        public double OffsetX
        {
            get => _offsetX;
            set => this.RaiseAndSetIfChanged(ref _offsetX, value);
        }

        public double OffsetY
        {
            get => _offsetY;
            set => this.RaiseAndSetIfChanged(ref _offsetY, value);
        }
    }
}
