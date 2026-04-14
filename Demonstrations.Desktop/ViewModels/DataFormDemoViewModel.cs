using Common.Controls.Models;
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

            FormConfig.Fields.Add("FullName", new DataFormFieldConfig
            {
                DisplayName = "Полное имя",
                Category = "Личные данные",
                Order = 1,
                Validation = new DataFormFieldValidation { MaxLength = 50 }
            });
            FormConfig.Fields.Add("Age", new DataFormFieldConfig
            {
                DisplayName = "Возраст",
                Category = "Личные данные",
                Order = 2,
                Validation = new DataFormFieldValidation { Min = 0, Max = 120 }
            });
            FormConfig.Fields.Add("Education", new DataFormFieldConfig
            {
                RowGroup = 1,
                Layout = FieldLayout.Horizontal,
            });
            FormConfig.Fields.Add("University", new DataFormFieldConfig
            {
                HideLabel = true,
                RowGroup = 1,
                Layout = FieldLayout.Horizontal,
            });
            FormConfig.CategoryOrders["Личные данные"] = 1;
            FormConfig.CategoryOrders["Основные"] = 2;
            FormConfig.CategoryOrders["Работа"] = 3;
            FormConfig.CategoryOrders["Образование"] = 4;
            FormConfig.CategoryCollapsible["Работа"] = true;
            FormConfig.CategoryCollapsible["Образование"] = true;

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
