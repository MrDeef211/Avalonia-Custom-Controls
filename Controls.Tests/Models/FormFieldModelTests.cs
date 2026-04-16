using Controls.Models;
using Controls.Tests.TestHelpers;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Controls.Tests.Models
{
    public class FormFieldModelTests
    {
        private PropertyInfo GetProperty(string name) => typeof(DataFormTestObject).GetProperty(name)!;

        [Fact]
        public void Constructor_Should_Set_Initial_Value_And_OriginalValue()
        {
            var target = new DataFormTestObject { Name = "Петр" };
            var prop = GetProperty(nameof(DataFormTestObject.Name));
            var model = new FormFieldModel(prop, target, false);

            model.Value.Should().Be("Петр");
            model.OriginalValue.Should().Be("Петр");
            model.ConvertedValue.Should().Be("Петр");
        }

        [Fact]
        public void Constructor_Should_Apply_DisplayName_From_Config()
        {
            var target = new DataFormTestObject();
            var prop = GetProperty(nameof(DataFormTestObject.Name));
            var config = new DataFormFieldConfig { DisplayName = "ФИО" };
            var model = new FormFieldModel(prop, target, false, config);

            model.DisplayName.Should().Be("ФИО");
        }

        [Fact]
        public void Constructor_Should_Apply_HideLabel_From_Config()
        {
            var target = new DataFormTestObject();
            var prop = GetProperty(nameof(DataFormTestObject.Name));
            var config = new DataFormFieldConfig { HideLabel = true };
            var model = new FormFieldModel(prop, target, false, config);

            model.HideLabel.Should().BeTrue();
            model.ShowLabel.Should().BeFalse();
            model.EditorColumn.Should().Be(0);
            model.EditorColumnSpan.Should().Be(2);
        }

        [Fact]
        public void Constructor_Should_Set_ReadOnly_From_Config_Override()
        {
            var target = new DataFormTestObject();
            var prop = GetProperty(nameof(DataFormTestObject.Name));
            var config = new DataFormFieldConfig { IsReadOnly = true };
            var model = new FormFieldModel(prop, target, false, config);

            model.IsReadOnly.Should().BeTrue();
        }

        [Fact]
        public void When_Value_Changed_Should_Set_IsTouched_And_Update_ConvertedValue()
        {
            var target = new DataFormTestObject { Name = "Старое" };
            var prop = GetProperty(nameof(DataFormTestObject.Name));
            var model = new FormFieldModel(prop, target, false);

            model.IsTouched.Should().BeFalse();
            model.Value = "Новое";
            model.IsTouched.Should().BeTrue();
            model.ConvertedValue.Should().Be("Новое");
        }

        [Fact]
        public void Validation_Should_Apply_Min_Max_For_Numeric()
        {
            var target = new DataFormTestObject { Age = 20 };
            var prop = GetProperty(nameof(DataFormTestObject.Age));
            var config = new DataFormFieldConfig
            {
                Validation = new DataFormFieldValidation { Min = 18, Max = 60 }
            };
            var model = new FormFieldModel(prop, target, false, config);

            model.Value = 15;
            model.ValidationError.Should().NotBeNullOrEmpty();

            model.Value = 25;
            model.ValidationError.Should().BeEmpty();
        }

        [Fact]
        public void Validation_Should_Apply_MaxLength_For_String()
        {
            var target = new DataFormTestObject();
            var prop = GetProperty(nameof(DataFormTestObject.Name));
            var config = new DataFormFieldConfig
            {
                Validation = new DataFormFieldValidation { MaxLength = 5 }
            };
            var model = new FormFieldModel(prop, target, false, config);

            model.Value = "слишкомдлинноеимя";
            model.ValidationError.Should().Contain("5");

            model.Value = "ОК";
            model.ValidationError.Should().BeEmpty();
        }

        [Fact]
        public void Validation_Should_Apply_RegexPattern()
        {
            var target = new DataFormTestObject();
            var prop = GetProperty(nameof(DataFormTestObject.Name));
            var config = new DataFormFieldConfig
            {
                Validation = new DataFormFieldValidation
                {
                    RegexPattern = @"^[A-Z][a-z]+$",
                    CustomErrorMessage = "Только латиница с заглавной"
                }
            };
            var model = new FormFieldModel(prop, target, false, config);

            model.Value = "john";
            model.ValidationError.Should().Be("Только латиница с заглавной");

            model.Value = "John";
            model.ValidationError.Should().BeEmpty();
        }

        [Fact]
        public void ResetToOriginal_Should_Restore_Value_And_Clear_ValidationError()
        {
            var target = new DataFormTestObject { Name = "Исходное" };
            var prop = GetProperty(nameof(DataFormTestObject.Name));
            var model = new FormFieldModel(prop, target, false);
            model.Value = "Изменённое";
            model.ValidationError = "Ошибка";

            model.ResetToOriginal();

            model.Value.Should().Be("Исходное");
            model.ValidationError.Should().BeEmpty();
            model.IsTouched.Should().BeFalse();
        }

        [Fact]
        public void Enum_SelectedEnumItem_Should_Sync_With_Value()
        {
            var target = new DataFormTestObject { Status = DataFormTestEnum.OptionB };
            var prop = GetProperty(nameof(DataFormTestObject.Status));
            var model = new FormFieldModel(prop, target, false);

            model.SelectedEnumItem.Should().NotBeNull();
            model.SelectedEnumItem!.Value.Should().Be(DataFormTestEnum.OptionB);

            var newItem = model.EnumDisplayItems!.First(i => (DataFormTestEnum)i.Value == DataFormTestEnum.OptionC);
            model.SelectedEnumItem = newItem;
            model.Value.Should().Be(DataFormTestEnum.OptionC);
        }

        [Fact]
        public void DateTime_DateValue_Should_Convert_Correctly()
        {
            var target = new DataFormTestObject { Created = new DateTime(2025, 3, 15) };
            var prop = GetProperty(nameof(DataFormTestObject.Created));
            var model = new FormFieldModel(prop, target, false);

            var expectedOffset = TimeZoneInfo.Local.GetUtcOffset(target.Created);
            model.DateValue.Should().Be(new DateTimeOffset(2025, 3, 15, 0, 0, 0, expectedOffset));
        }
    }
}
