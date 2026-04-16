using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Xunit.Sdk;

namespace Controls.Tests.TestHelpers
{
    public enum DataFormTestEnum
    {
        [Description("Вариант А")]
        OptionA,
        [Description("Вариант Б")]
        OptionB,
        [Description("Вариант В")]
        OptionC
    }

    public class DataFormTestObject
    {
        [Category("Основные")]
        [DisplayName("Имя")]
        public string Name { get; set; } = "Иван";

        [Category("Основные")]
        [Range(18, 100, ErrorMessage = "Возраст должен быть от 18 до 100")]
        public int Age { get; set; } = 30;

        [Category("Дополнительно")]
        public bool IsActive { get; set; } = true;

        [Category("Дополнительно")]
        public DateTime Created { get; set; } = new DateTime(2025, 1, 1);

        [Category("Статус")]
        public DataFormTestEnum Status { get; set; } = DataFormTestEnum.OptionA;

        [Category("Основные")]
        public double Weight { get; set; } = 70.5;

        // Свойство без публичного сеттера (readonly)
        public string ReadOnlyProp => "Только чтение";

        // Приватный сеттер
        public string PrivateSetProp { get; private set; } = "private";

        // Свойство с валидацией через атрибуты
        [StringLength(10, ErrorMessage = "Максимум 10 символов")]
        [Category("Основные")]
        public string ShortString { get; set; } = "коротко";

        [Required(ErrorMessage = "Имя обязательно")]
        public string RequiredName { get; set; } = "John";
    }
}
