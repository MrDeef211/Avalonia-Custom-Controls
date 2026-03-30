using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Controls.Models
{
    public struct AutoDouble
    {
        public bool IsAuto { get; private set; }
        public double Value { get; }

        public AutoDouble(double value)
        {
            IsAuto = false;
            Value = value;
        }

        public static AutoDouble Auto => new AutoDouble { IsAuto = true };

        public static implicit operator AutoDouble(double value) => new AutoDouble(value);

        public static implicit operator double(AutoDouble autoDouble) => autoDouble.IsAuto ? throw new InvalidOperationException("Value is Auto") : autoDouble.Value;
    }
}
