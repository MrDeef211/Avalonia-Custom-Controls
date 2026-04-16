using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Controls.Tests.TestHelpers
{
    public enum TestEnum
    {
        [Description("Активен")]
        Active,

        [Description("Неактивен")]
        Inactive,

        [Description("В ожидании")]
        Pending
    }

    public class PropertyGridTestObject
    {
        [Category("Основные")]
        public string Name { get; set; } = "Default Name";

        public int Age { get; set; } = 30;

        [Description("Флаг активности")]
        public bool IsActive { get; set; } = true;

        public DateTime Created { get; set; } = new DateTime(2025, 1, 1);

        [Category("Дополнительно")]
        public TestEnum Status { get; set; } = TestEnum.Active;

        public double Weight { get; set; } = 75.5;

        public string? NullableString { get; set; } = "test";

        // Свойство только для чтения (нет set)
        public string ReadOnlyProperty => "ReadOnly";

        // Приватный set
        public string PrivateSetProperty { get; private set; } = "private";

        // Свойство с public set, но без атрибутов
        public string DescriptionProperty { get; set; } = "desc";

        // Числовое свойство для проверки валидации (не используется в тестах напрямую)
        public int PositiveNumber { get; set; } = 5;
    }
}
