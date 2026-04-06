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
        private DemoObject _selectedObject;
        private bool _isReadOnly;
        private string _statusMessage = string.Empty;
        private bool _hasChanges;
        private string _error = string.Empty;

        public PropertyGridDemoViewModel()
        {
            Title = "PropertyGrid - Редактор свойств";

            NewObject();

            // Команды
            NewObjectCommand = ReactiveCommand.Create(NewObject);
            MakeReadOnlyCommand = ReactiveCommand.Create(() => { IsReadOnly = !IsReadOnly; });
            ClearChangesCommand = ReactiveCommand.Create(ClearChanges);

            this.WhenAnyValue(x => x.HasChanges)
                .Subscribe(hasChanges => StatusMessage = hasChanges ? "Есть несохранённые изменения" : "Нет изменений");
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

        public bool HasChanges
        {
            get => _hasChanges;
            set => this.RaiseAndSetIfChanged(ref _hasChanges, value);
        }

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
            SelectedObject = new DemoObject();
            HasChanges = false; 
        }

        private void ClearChanges()
        {
            HasChanges = false; 
        }
    }
}