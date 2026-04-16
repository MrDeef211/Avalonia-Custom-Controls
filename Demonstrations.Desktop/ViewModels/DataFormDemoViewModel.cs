using Controls.Models;
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
        private RichDemoObject _selectedObject;

        public DataFormConfig FormConfig { get; } = new();

        public DataFormDemoViewModel()
        {
            Title = "DataForm - Форма данных";
            NewObject();

            FormConfig.SetFieldRule("FullName", new DataFormFieldConfig
            {
                DisplayName = "Полное имя",
                Category = "Личные данные",
                Order = 1,
                Validation = new DataFormFieldValidation { MaxLength = 50 }
            });
            FormConfig.SetFieldRule("Age", new DataFormFieldConfig
            {
                DisplayName = "Возраст",
                Category = "Личные данные",
                Order = 2,
                Validation = new DataFormFieldValidation { Min = 0, Max = 120 }
            });
            FormConfig.SetFieldRule("Education", new DataFormFieldConfig
            {
                RowGroup = 1,
            });
            FormConfig.SetFieldRule("University", new DataFormFieldConfig
            {
                HideLabel = true,
                RowGroup = 1,
            });
            FormConfig.SetCategoryOrder("Личные данные", 1);
            FormConfig.SetCategoryOrder("Основные", 2);
            FormConfig.SetCategoryOrder("Работа", 3);
            FormConfig.SetCategoryOrder("Образование", 4);
            FormConfig.SetCategoryCollapsible("Работа", true);
            FormConfig.SetCategoryCollapsible("Образование", true, true);


            SaveCommand = ReactiveCommand.Create<object?>(OnSave);
        }

        private bool _isReadOnly;
        public RichDemoObject SelectedObject
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

        private void NewObject() => SelectedObject = new RichDemoObject();

        public ReactiveCommand<object?, Unit> SaveCommand { get; }

        private async void OnSave(object? savedObject)
        {
            StatusMessage = $"Сохранён объект: {savedObject}";
            await Task.Delay(3000);
            StatusMessage = string.Empty;
        }

    }
}
