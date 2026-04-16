using System;
using System.Collections.Generic;
using System.Text;

namespace Controls.Models
{
    /// <summary>
    /// Представляет элемент перечисления для отображения в ComboBox.
    /// </summary>
    public class EnumItem
    {
        /// <summary>
        /// Исходное значение перечисления.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Отображаемое имя.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EnumItem"/>.
        /// </summary>
        /// <param name="value">Значение перечисления.</param>
        /// <param name="displayName">Отображаемое имя.</param>
        public EnumItem(object value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }
    }
}
