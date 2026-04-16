using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace Controls.Models
{
    public class ChartDataBase
    {
        private ConcurrentDictionary<double, double> _chart;
        public Dictionary<double, double> Chart
        {
            get => new Dictionary<double, double>(_chart);
            set => _chart = new ConcurrentDictionary<double, double>(value);
        }

        public ChartDataBase(Dictionary<double, double> Chart) =>
            this.Chart = Chart == null ? throw new ArgumentNullException(nameof(Chart)) : Chart.ToDictionary(x => x.Key, x => x.Value);

        public ChartDataBase() => Chart = new();



        /// <summary>
        /// Добавить точку на график по координатам
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(double key, double value) =>
            _chart.TryAdd(key, value);

        /// <summary>
        /// Добавление новой точки в конец (максимальный ключ плюс один)
        /// </summary>
        /// <param name="value"></param>
        public void Add(double value) =>
            Add((_chart?.Keys.DefaultIfEmpty(-1).Max() ?? -1) + 1, value);

        public void Remove(double key) =>
            _chart.TryRemove(key, out _);

        public int Count => _chart.Count;
    }
}
