using ReactiveUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Controls.Models
{
    public class PieChartDataBase : ReactiveObject
    {
        private ConcurrentDictionary<string, double> _chart;
        public Dictionary<string, double> Chart
        {
            get => new Dictionary<string, double>(_chart);
            set
            {
                var newDict = new ConcurrentDictionary<string, double>(value ?? throw new ArgumentNullException(nameof(value)));
                this.RaiseAndSetIfChanged(ref _chart, newDict);
                this.RaisePropertyChanged(nameof(Count));
                this.RaisePropertyChanged(nameof(Sum));
            }
        }

        public PieChartDataBase(Dictionary<string, double> chart) =>
            _chart = new ConcurrentDictionary<string, double>(chart ?? throw new ArgumentNullException(nameof(chart)));

        public PieChartDataBase(ConcurrentDictionary<string, double> chart) =>
            _chart = new ConcurrentDictionary<string, double>(chart ?? throw new ArgumentNullException(nameof(chart)));

        public PieChartDataBase() => Chart = new();

        public double Sum() => Chart.Select(x => x.Value).Sum(); 

        public int Count { get => Chart.Count; }

        /// <summary>
        /// Добавить точку на график по координатам
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, double value)
        {
            if (_chart.TryAdd(key, value))
            {
                this.RaisePropertyChanged(nameof(Chart));
                this.RaisePropertyChanged(nameof(Count));
            }
        }

        /// <summary>
        /// Массовое добавление данных
        /// </summary>
        public void AddRange(IEnumerable<KeyValuePair<string, double>> items)
        {
            if (items == null) return;

            bool changed = false;
            foreach (var item in items)
            {
                if (_chart.TryAdd(item.Key, item.Value))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                this.RaisePropertyChanged(nameof(Chart));
                this.RaisePropertyChanged(nameof(Count));
            }
        }

        public void Remove(string key)
        {
            if (_chart.TryRemove(key, out _))
            {
                this.RaisePropertyChanged(nameof(Chart));
                this.RaisePropertyChanged(nameof(Count));
            }
        }
    }
}
