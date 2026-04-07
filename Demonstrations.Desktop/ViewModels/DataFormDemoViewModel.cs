using Demonstrations.Desktop.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;

namespace Demonstrations.Desktop.ViewModels
{
    public class DataFormDemoViewModel : PageViewModelBase
    {
        private DemoObject _selectedObject;
        private bool _isReadOnly;
        private string _statusMessage = string.Empty;
        private string _error = string.Empty;
        private bool _hasChanges;

        public DataFormDemoViewModel()
        {
            Title = "DataForm - Форма данных";
            NewObject();
        }

        public DemoObject SelectedObject
        {
            get => _selectedObject;
            set => this.RaiseAndSetIfChanged(ref _selectedObject, value);
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => this.RaiseAndSetIfChanged(ref _isReadOnly, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public string Error
        {
            get => _error;
            set => this.RaiseAndSetIfChanged(ref _error, value);
        }

        public bool HasChanges
        {
            get => _hasChanges;
            set => this.RaiseAndSetIfChanged(ref _hasChanges, value);
        }

        private void NewObject() => SelectedObject = new DemoObject();

    }
}
