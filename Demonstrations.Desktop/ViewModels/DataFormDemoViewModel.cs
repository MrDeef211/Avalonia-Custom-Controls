using Controls.Models;
using Demonstrations.Desktop.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Demonstrations.Desktop.ViewModels
{
    public class DataFormDemoViewModel : PageViewModelBase
    {
        public enum DemoVariant
        {
            AttributesOnly,      // Только атрибуты
            ConfigOnly,          // Только конфиг
            Mixed,               // Смешанный
            NoConfig             // Без конфигурации
        }

        private RichDemoObject _attributesObject;
        private ConfigOnlyObject _configObject;
        private MixedObject _mixedObject;
        private NoConfigObject _noConfigObject;

        private DemoVariant _selectedVariant;
        private object? _selectedObject;
        private DataFormConfig? _currentFormConfig;

        public ObservableCollection<DemoVariant> AvailableVariants { get; } = new()
        {
            DemoVariant.AttributesOnly,
            DemoVariant.ConfigOnly,
            DemoVariant.Mixed,
            DemoVariant.NoConfig
        };

        public DataFormDemoViewModel()
        {
            Title = "DataForm - Форма данных";

            // Инициализация объектов
            _attributesObject = new RichDemoObject();
            _configObject = new ConfigOnlyObject();
            _mixedObject = new MixedObject();
            _noConfigObject = new NoConfigObject();

            // Начальный вариант
            SelectedVariant = DemoVariant.AttributesOnly;

            SaveCommand = ReactiveCommand.Create<object?>(OnSave);
        }

        public DemoVariant SelectedVariant
        {
            get => _selectedVariant;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedVariant, value);
                UpdateSelectedObjectAndConfig();
            }
        }

        public object? SelectedObject
        {
            get => _selectedObject;
            private set => this.RaiseAndSetIfChanged(ref _selectedObject, value);
        }

        public DataFormConfig? CurrentFormConfig
        {
            get => _currentFormConfig;
            private set => this.RaiseAndSetIfChanged(ref _currentFormConfig, value);
        }

        private void UpdateSelectedObjectAndConfig()
        {
            switch (SelectedVariant)
            {
                case DemoVariant.AttributesOnly:
                    SelectedObject = _attributesObject;
                    CurrentFormConfig = null;
                    break;
                case DemoVariant.ConfigOnly:
                    SelectedObject = _configObject;
                    CurrentFormConfig = CreateConfigForConfigOnly();
                    break;
                case DemoVariant.Mixed:
                    SelectedObject = _mixedObject;
                    CurrentFormConfig = CreateConfigForMixed();
                    break;
                case DemoVariant.NoConfig:
                    SelectedObject = _noConfigObject;
                    CurrentFormConfig = null;
                    break;
            }
        }

        private DataFormConfig CreateConfigForConfigOnly()
        {
            var config = new DataFormConfig();

            config.SetFieldRule("FullName", new DataFormFieldConfig
            {
                DisplayName = "Полное имя",
                Category = "Личная информация",
                Order = 1,
                Validation = new DataFormFieldValidation { MaxLength = 100 }
            });
            config.SetFieldRule("Age", new DataFormFieldConfig
            {
                DisplayName = "Возраст",
                Category = "Личная информация",
                Order = 2,
                Validation = new DataFormFieldValidation { Min = 18, Max = 70 }
            });
            config.SetFieldRule("BirthDate", new DataFormFieldConfig
            {
                DisplayName = "Дата рождения",
                Category = "Личная информация",
                Order = 3
            });

            config.SetFieldRule("Email", new DataFormFieldConfig
            {
                DisplayName = "Электронная почта",
                Category = "Контакты",
                Order = 4,
                Validation = new DataFormFieldValidation
                {
                    RegexPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    CustomErrorMessage = "Некорректный email"
                }
            });
            config.SetFieldRule("Phone", new DataFormFieldConfig
            {
                DisplayName = "Телефон",
                Category = "Контакты",
                Order = 5,
                Validation = new DataFormFieldValidation
                {
                    RegexPattern = @"^\+7\s?\d{3}\s?\d{3}\s?\d{2}\s?\d{2}$",
                    CustomErrorMessage = "Формат: +7 XXX XXX XX XX"
                }
            });

            config.SetFieldRule("Department", new DataFormFieldConfig
            {
                DisplayName = "Отдел",
                Category = "Работа",
                Order = 6
            });
            config.SetFieldRule("Salary", new DataFormFieldConfig
            {
                DisplayName = "Зарплата",
                Category = "Работа",
                Order = 7,
                Validation = new DataFormFieldValidation { Min = 0 }
            });
            config.SetFieldRule("IsFullTime", new DataFormFieldConfig
            {
                DisplayName = "Полная занятость",
                Category = "Работа",
                Order = 8
            });
            config.SetFieldRule("HireDate", new DataFormFieldConfig
            {
                DisplayName = "Дата найма",
                Category = "Работа",
                Order = 9
            });

            config.SetFieldRule("Notes", new DataFormFieldConfig
            {
                DisplayName = "Заметки",
                Category = "Прочее",
                Order = 10,
                Validation = new DataFormFieldValidation { MaxLength = 500 }
            });

            config.SetCategoryOrder("Личная информация", 1);
            config.SetCategoryOrder("Контакты", 2);
            config.SetCategoryOrder("Работа", 3);
            config.SetCategoryOrder("Прочее", 4);

            config.SetCategoryCollapsible("Контакты", true);
            config.SetCategoryCollapsible("Прочее", true, true); 

            return config;
        }

        private DataFormConfig CreateConfigForMixed()
        {
            var config = new DataFormConfig();

            config.SetFieldRule("FullName", new DataFormFieldConfig
            {
                DisplayName = "ФИО (переопределено конфигом)",
                Category = "Основные данные"
            });

            config.SetFieldValidation("Age", new DataFormFieldValidation
            {
                Min = 18,
                Max = 65,
                CustomErrorMessage = "Возраст должен быть от 18 до 65 (переопределено)"
            });

            config.SetFieldRule("BirthDate", new DataFormFieldConfig
            {
                Category = "Основные данные"
            });

            config.SetFieldRule("Position", new DataFormFieldConfig
            {
                DisplayName = "Должность",
                Category = "Работа"
            });
            config.SetFieldRule("Salary", new DataFormFieldConfig
            {
                DisplayName = "Оклад",
                Category = "Работа"
            });
            config.SetFieldRule("IsRemote", new DataFormFieldConfig
            {
                DisplayName = "Удалённая работа",
                Category = "Работа"
            });

            config.SetFieldRule("Skills", new DataFormFieldConfig
            {
                Category = "Профессиональные",
                Order = 1
            });
            config.SetFieldRule("Experience", new DataFormFieldConfig
            {
                Category = "Профессиональные",
                Order = 2
            });
            config.SetFieldRule("Email", new DataFormFieldConfig
            {
                Order = 1,
                RowGroup = 1,
                DisplayName = "Почта / Телефон"
                
            });
            config.SetFieldRule("Phone", new DataFormFieldConfig
            {
                Order = 2,
                RowGroup = 1,
                HideLabel = true
            });

            config.SetCategoryOrder("Основные данные", 1);
            config.SetCategoryOrder("Работа", 2);
            config.SetCategoryOrder("Профессиональные", 3);

            return config;
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage { get => _statusMessage; set => this.RaiseAndSetIfChanged(ref _statusMessage, value); }

        private string _error = string.Empty;
        public string Error { get => _error; set => this.RaiseAndSetIfChanged(ref _error, value); }

        private bool _hasChanges;
        public bool HasChanges { get => _hasChanges; set => this.RaiseAndSetIfChanged(ref _hasChanges, value); }

        public ReactiveCommand<object?, Unit> SaveCommand { get; }

        private async void OnSave(object? savedObject)
        {
            StatusMessage = $"Сохранён объект: {savedObject}";
            await Task.Delay(3000);
            StatusMessage = string.Empty;
        }
    }
}
