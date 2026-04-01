using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Controls.Models
{
    public class ChartGenerator
    {
        public ChartDataBase Generate()
        {
            var dB = new ChartDataBase();
            var rand = new Random();
            dB.Add(-200, 0);
            for (int i = -199; i < 200; i++)
            {
                var dy = rand.Next(0, 100);
                dB.Add(i, dB.Chart[i - 1] + Math.Cos(dy) * 100 + (-dB.Chart[i - 1]) / 10 * Math.Abs(Math.Cos(dy)));
            }
            return dB;
        }
    }
}
