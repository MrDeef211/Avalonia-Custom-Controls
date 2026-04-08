using Demonstrations.Desktop.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Demonstrations.Desktop.ViewModels
{
    public class DataFormDemoViewModel : PageViewModelBase
    {
        private DemoObject _selectedObject;
        public DataFormDemoViewModel()
        {
            Title = "DataForm - Форма данных";
            NewObject();

            SaveCommand = ReactiveCommand.Create<object?>(OnSave);
        }

        private bool _isReadOnly;
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

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        private string _error = string.Empty;
        public string Error
        {
            get => _error;
            set => this.RaiseAndSetIfChanged(ref _error, value);
        }

        private bool _hasChanges;
        public bool HasChanges
        {
            get => _hasChanges;
            set => this.RaiseAndSetIfChanged(ref _hasChanges, value);
        }

        private void NewObject() => SelectedObject = new DemoObject();

        public ReactiveCommand<object?, Unit> SaveCommand { get; }

        private async void OnSave(object? savedObject)
        {
            StatusMessage = $"Сохранён объект: {savedObject}";
            await Task.Delay(3000);
            StatusMessage = string.Empty;
        }

    }
}
