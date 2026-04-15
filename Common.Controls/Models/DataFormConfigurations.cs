using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Controls.Models
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
    /// Конфигурация формы <see cref="DataFormControl"/>.
    /// </summary>
    public class DataFormConfig
    {
        /// <summary>
        /// Определяет, могут ли категории сворачиваться пользователем.
        /// Ключ — имя категории, значение — <c>true</c>, если категория сворачиваема.
        /// </summary>
        public Dictionary<string, bool> CategoryCollapsible { get; set; } = new();

        /// <summary>
        /// Начальное состояние развёрнутости категорий.
        /// Ключ — имя категории, значение — <c>true</c>, если категория развёрнута по умолчанию.
        /// </summary>
        public Dictionary<string, bool> CategoryExpanded { get; set; } = new();

        /// <summary>
        /// Конфигурации отдельных полей по имени свойства.
        /// </summary>
        public Dictionary<string, DataFormFieldConfig> Fields { get; set; } = new();

        /// <summary>
        /// Порядок отображения категорий.
        /// Ключ — имя категории, значение — порядковый номер (меньше — раньше).
        /// </summary>
        public Dictionary<string, int> CategoryOrders { get; set; } = new();

        /// <summary>
        /// Устанавливает порядок отображения категории.
        /// </summary>
        /// <param name="categoryName">Имя категории.</param>
        /// <param name="order">Порядковый номер.</param>
        public void SetCategoryOrder(string categoryName, int order)
        {
            var normalized = categoryName?.Trim() ?? throw new ArgumentNullException(nameof(categoryName));
            CategoryOrders[normalized] = order;
        }
    }
}
