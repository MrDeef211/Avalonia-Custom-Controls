using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System;
using System.Reactive;
using System.Reactive.Linq;
using Demonstrations.Desktop.Models;

namespace Demonstrations.Desktop.ViewModels
{
    public class PropertyGridDemoViewModel : PageViewModelBase
    {

        public PropertyGridDemoViewModel()
        {
            Title = "PropertyGrid - Редактор свойств";

            NewObject();

            // Команды
            NewObjectCommand = ReactiveCommand.Create(NewObject);
            MakeReadOnlyCommand = ReactiveCommand.Create(() => { IsReadOnly = !IsReadOnly; });
        }

        private RichDemoObject _selectedObject;
        public RichDemoObject SelectedObject
        {
            get => _selectedObject;
            set => this.RaiseAndSetIfChanged(ref _selectedObject, value);
        }

        private bool _isReadOnly;
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

        public ReactiveCommand<Unit, Unit> NewObjectCommand { get; }
        public ReactiveCommand<Unit, Unit> MakeReadOnlyCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearChangesCommand { get; }

        private void NewObject()
        {
            SelectedObject = new RichDemoObject();
        }
    }
}