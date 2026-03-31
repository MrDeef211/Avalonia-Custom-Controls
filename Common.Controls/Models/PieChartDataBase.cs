using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Controls.Models
{
    public class PieChartDataBase
    {
        private ConcurrentDictionary<string, double> _chart;
        public Dictionary<string, double> Chart
        {
            get => new Dictionary<string, double>(_chart);
            set => _chart = new ConcurrentDictionary<string, double>(value);
        }

        public PieChartDataBase(Dictionary<string, double> Chart) =>
            this.Chart = Chart ?? throw new ArgumentNullException(nameof(Chart));

        public PieChartDataBase() => Chart = new();

        public double Sum() => Chart.Select(x => x.Value).Sum(); 

        public int Count { get => Chart.Count; }

        /// <summary>
        /// Добавить точку на график по координатам
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, double value) =>
            _chart.TryAdd(key, value);

        public void Remove(string key) =>
            _chart.TryRemove(key, out _);

    }
}
