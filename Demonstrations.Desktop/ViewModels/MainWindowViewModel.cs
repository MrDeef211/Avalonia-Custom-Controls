using Avalonia;
using Avalonia.Media.TextFormatting;
using Common.Controls;
using Common.Controls.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace Demonstrations.Desktop.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel() 
        {
            var canExecuteTest = this.WhenAnyValue(x => x.TestCommandIsActive);
            ChartData = new();

            ClickCommand = ReactiveCommand.Create(() =>
            {
                TestCommandIsActive = !TestCommandIsActive;
            });

            TestCommand = ReactiveCommand.Create(() => { }, canExecuteTest);

            DrawCommand = ReactiveCommand.Create(() =>
            {
                var dB = new ChartDataBase();
                for (int i = 0; i < 100; i++)
                {
                    dB.Add(new Random().NextDouble() * 10);
                }
                ChartData = dB;
            }, canExecuteTest);
        }

        private bool _testCommandIsActive = true;
        public bool TestCommandIsActive
        {
            get => _testCommandIsActive;
            set => this.RaiseAndSetIfChanged(ref _testCommandIsActive, value);
        }

        private bool _isFill;
        public bool IsFill
        {
            get => _isFill;
            set => this.RaiseAndSetIfChanged(ref _isFill, value);
        }

        private bool _grid;
        public bool Grid
        {
            get => _grid;
            set => this.RaiseAndSetIfChanged(ref _grid, value);
        }

        private ChartDataBase  _chartData;
        public ChartDataBase ChartData
        {
            get => _chartData;
            set => this.RaiseAndSetIfChanged(ref _chartData, value);
        }

        public IEnumerable<ChartStyle> AllChartStyles =>
            Enum.GetValues(typeof(ChartStyle)).Cast<ChartStyle>();

        private ChartStyle _selectedChartStyle = ChartStyle.Line;
        public ChartStyle SelectedChartStyle
        {
            get => _selectedChartStyle;
            set => this.RaiseAndSetIfChanged(ref _selectedChartStyle, value);
        }

        public ReactiveCommand<Unit,Unit> ClickCommand { get; }

        public ReactiveCommand<Unit, Unit> TestCommand { get; }

        public ReactiveCommand<Unit, Unit> DrawCommand { get; }
    }
}
