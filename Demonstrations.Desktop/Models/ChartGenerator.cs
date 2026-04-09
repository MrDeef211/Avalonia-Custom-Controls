using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Controls.Models
{
    public class ChartGenerator
    {
        public ChartDataBase Generate(double startPoint, int startIndex, int endIndex)
        {
            var dB = new ChartDataBase();
            var rand = new Random();
            dB.Add(startIndex, startPoint);
            for (int i = startIndex + 1; i < endIndex + 1; i++)
            {
                var dy = rand.Next(0, 100);
                dB.Add(i, dB.Chart[i - 1] + Math.Cos(dy) * 100 + (-dB.Chart[i - 1]) / 10 * Math.Abs(Math.Cos(dy)));
            }
            return dB;
        }

        public ChartDataBase Generate() => Generate(0, -200, 199);

        public ChartDataBase Generate(double startPoint) => Generate(startPoint, -200, 199);

        public ChartDataBase Generate(double startPoint, int startIndex) => Generate(startPoint, startIndex, startIndex + 499);

        public ChartDataBase Generate(int startIndex) => Generate(0, startIndex, startIndex + 499);

        public ChartDataBase Generate(int startIndex, int endIndex) => Generate(0, startIndex, endIndex);
    }
}
