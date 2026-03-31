using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demonstrations.Desktop.ViewModels
{
    public abstract class PageViewModelBase : ViewModelBase
    {
        private string _title;
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }
    }
}
