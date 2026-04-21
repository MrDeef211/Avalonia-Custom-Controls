using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Controls.Models
{
    /// <summary>
    /// Конфигурация валидации поля.
    /// </summary>
    public class DataFormFieldValidation
    {
        /// <summary>
        /// Минимальное допустимое значение (для числовых типов).
        /// </summary>
        public double? Min { get; set; }

        /// <summary>
        /// Максимальное допустимое значение (для числовых типов).
        /// </summary>
        public double? Max { get; set; }

        /// <summary>
        /// Максимальная длина строки.
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Регулярное выражение для проверки строки.
        /// </summary>
        public string? RegexPattern { get; set; }

        /// <summary>
        /// Пользовательское сообщение об ошибке.
        /// </summary>
        public string? CustomErrorMessage { get; set; }
    }

    /// <summary>
    /// Конфигурация отображения и поведения отдельного поля формы.
    /// </summary>
    public class DataFormFieldConfig
    {
        /// <summary>
        /// Отображаемое имя поля. Если не задано, используется имя свойства или атрибут DisplayName.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Категория, в которую попадает поле.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Указывает, доступно ли поле только для чтения.
        /// </summary>
        public bool? IsReadOnly { get; set; }

        /// <summary>
        /// Указывает, обязательно ли поле.
        /// </summary>
        public bool? IsRequired { get; set; }   

        /// <summary>
        /// Указывает, отображать ли поле в форме.
        /// </summary>
        public bool IsBrowsable { get; set; } = true;

        /// <summary>
        /// Указывает, развёрнута ли категория по умолчанию.
        /// </summary>
        public bool IsCategoryExpanded { get; set; } = true;

        /// <summary>
        /// Порядок отображения поля внутри категории.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Порядок отображения категории. Если не задан, используется общий порядок категорий.
        /// </summary>
        public int? CategoryOrder { get; set; }

        /// <summary>
        /// Скрывать ли метку поля.
        /// </summary>
        public bool HideLabel { get; set; }

        /// <summary>
        /// Идентификатор группы для объединения полей в одну строку.
        /// Поля с одинаковым RowGroup помещаются в одну строку.
        /// Значение -1 означает отсутствие группировки.
        /// </summary>
        public int RowGroup { get; set; } = -1;

        /// <summary>
        /// Конфигурация валидации поля.
        /// </summary>
        public DataFormFieldValidation? Validation { get; set; }
    }

    /// <summary>
    /// Конфигурация формы <see cref="DataForm"/>.
    /// </summary>
    public class DataFormConfig
    {

        protected internal readonly Dictionary<string, bool> _categoryCollapsible = new();
        protected internal readonly Dictionary<string, bool> _categoryExpanded = new();
        protected internal readonly Dictionary<string, DataFormFieldConfig> _fields = new();
        protected internal readonly Dictionary<string, int> _categoryOrders = new();

        /// <summary>
        /// Определяет, могут ли категории сворачиваться пользователем.
        /// Ключ — имя категории, значение — <c>true</c>, если категория сворачиваема.
        /// </summary>
        public IReadOnlyDictionary<string, bool> CategoryCollapsible => _categoryCollapsible;

        /// <summary>
        /// Начальное состояние развёрнутости категорий.
        /// Ключ — имя категории, значение — <c>true</c>, если категория развёрнута по умолчанию.
        /// </summary>
        public IReadOnlyDictionary<string, bool> CategoryExpanded => _categoryExpanded;

        /// <summary>
        /// Конфигурации отдельных полей по имени свойства.
        /// </summary>
        public IReadOnlyDictionary<string, DataFormFieldConfig> Fields => _fields;

        /// <summary>
        /// Порядок отображения категорий.
        /// Ключ — имя категории, значение — порядковый номер (меньше — раньше).
        /// </summary>
        public IReadOnlyDictionary<string, int> CategoryOrders => _categoryOrders;

        /// <summary>
        /// Добавляет конфигурации отдельных полей по имени свойства.
        /// </summary>
        /// <remarks>
        /// Добавляет новые правила к старым.
        /// </remarks>
        /// <param name="fieldName">Имя поля.</param>
        /// <param name="config">Правила.</param>
        public void AddFieldRule(string fieldName, DataFormFieldConfig config)
        {
            var normalized = fieldName?.Trim() ?? throw new ArgumentNullException(nameof(fieldName));
            if (!Fields.ContainsKey(normalized))
                _fields.TryAdd(normalized, config);
            else
                _fields[normalized] = MergeConfig(Fields[normalized], config);
        }

        /// <summary>
        /// Добавляет правила валидации отдельных полей по имени свойства.
        /// </summary>
        /// <remarks>
        /// Добавляет новые правила к старым.
        /// </remarks>
        /// <param name="fieldName">Имя поля.</param>
        /// <param name="validation">Правила.</param>
        public void AddFieldValidation(string fieldName, DataFormFieldValidation validation) => AddFieldRule(fieldName, new DataFormFieldConfig { Validation = validation });

        /// <summary>
        /// Устанавливает конфигурации отдельных полей по имени свойства.
        /// </summary>
        /// <remarks>
        /// Заменяет старое правило новым.
        /// </remarks>
        /// <param name="fieldName">Имя поля.</param>
        /// <param name="config">Правило.</param>
        public void SetFieldRule(string fieldName, DataFormFieldConfig config)
        {
            var normalized = fieldName?.Trim() ?? throw new ArgumentNullException(nameof(fieldName));
            if (!Fields.ContainsKey(normalized))
                _fields.TryAdd(normalized, config);
            else
                _fields[normalized] = config;
        }

        /// <summary>
        /// Устанавливает правила валидации отдельных полей по имени свойства.
        /// </summary>
        /// <remarks>
        /// Заменяет старое правило новым.
        /// </remarks>
        /// <param name="fieldName">Имя поля.</param>
        /// <param name="config">Правило.</param>
        public void SetFieldValidation(string fieldName, DataFormFieldValidation validation)
        {
            var normalized = fieldName?.Trim() ?? throw new ArgumentNullException(nameof(fieldName));
            if (!Fields.ContainsKey(normalized))
                _fields.TryAdd(normalized, new DataFormFieldConfig { Validation = validation });
            else
                _fields[normalized].Validation = validation;
        }

        /// <summary>
        /// Устанавливает порядок отображения категории.
        /// </summary>
        /// <param name="categoryName">Имя категории.</param>
        /// <param name="order">Порядковый номер.</param>
        public void SetCategoryOrder(string categoryName, int order)
        {
            var normalized = categoryName?.Trim() ?? throw new ArgumentNullException(nameof(categoryName));
            _categoryOrders[normalized] = order;
        }

        /// <summary>
        /// Устанавливает возможность схлопывания категории.
        /// </summary>
        /// <param name="categoryName">Имя категории.</param>
        /// <param name="collapsible">Возможность схлопывания.</param>
        /// <param name="defaultСollapsed">Схлонуто по умолчанию.</param>
        public void SetCategoryCollapsible(string categoryName, bool collapsible, bool defaultСollapsed)
        {
            var normalized = categoryName?.Trim() ?? throw new ArgumentNullException(nameof(categoryName));
            _categoryCollapsible[normalized] = collapsible;
            if (collapsible)
                _categoryExpanded[normalized] = !defaultСollapsed;
        }

        /// <summary>
        /// Устанавливает возможность схлопывания категории.
        /// </summary>
        /// <param name="categoryName">Имя категории.</param>
        /// <param name="collapsible">Возможность схлопывания.</param>
        public void SetCategoryCollapsible(string categoryName, bool collapsible) => SetCategoryCollapsible(categoryName, collapsible, false);

        private DataFormFieldConfig MergeConfig(DataFormFieldConfig first, DataFormFieldConfig second)
        {
            if (first == null || second == null)
                return first ?? second;

            var fV = first.Validation;
            var sV = second.Validation;

            return new DataFormFieldConfig
            {
                DisplayName = second.DisplayName ?? first.DisplayName,
                Category = second.Category ?? first.Category,
                IsReadOnly = second.IsReadOnly ?? first.IsReadOnly,
                IsBrowsable = second.IsBrowsable != default ? second.IsBrowsable : first.IsBrowsable,
                IsCategoryExpanded = second.IsCategoryExpanded ? first.IsCategoryExpanded : second.IsCategoryExpanded,
                Order = second.Order != 0 ? second.Order : first.Order,
                CategoryOrder = second.CategoryOrder ?? first.CategoryOrder,
                HideLabel = second.HideLabel ? second.HideLabel : first.HideLabel,
                RowGroup = second.RowGroup != -1 ? second.RowGroup : first.RowGroup,

                Validation = new DataFormFieldValidation
                {
                    Min = sV?.Min ?? fV?.Min,
                    Max = sV?.Max ?? fV?.Max,
                    MaxLength = sV?.MaxLength ?? fV?.MaxLength,
                    RegexPattern = sV?.RegexPattern ?? fV?.RegexPattern,
                    CustomErrorMessage = sV?.CustomErrorMessage ?? fV?.CustomErrorMessage
                }
            };            
        }
    }
}
