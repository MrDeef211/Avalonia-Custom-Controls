using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Media.TextFormatting;
using Common.Controls;
using Common.Controls.Models;
using ReactiveUI;
using ReactiveUI.Validation;
using ReactiveUI.Validation.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

namespace Demonstrations.Desktop.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Pages = new ObservableCollection<PageViewModelBase>
            {
                new ChartDemoViewModel(),
                new ChartDemoViewModel(),
                new PieChartDemoViewModel(),
                new RangeSliderDemoViewModel(),
                new PropertyGridDemoViewModel()
            };

            CurrentPage = Pages[0];

            NavigateCommand = ReactiveCommand.Create<PageViewModelBase>(page => CurrentPage = page);
        }

        private PageViewModelBase _currentPage;
        public PageViewModelBase CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        public ObservableCollection<PageViewModelBase> Pages { get; }

        public ReactiveCommand<PageViewModelBase, Unit> NavigateCommand { get; }
    }
}
