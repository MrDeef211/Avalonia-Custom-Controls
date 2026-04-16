using Controls.Models;
using Controls.Tests.TestHelpers;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Controls.Tests.Models
{
    public class PropertyItemModelTests
    {
        private PropertyInfo GetProperty(string name) => typeof(PropertyGridTestObject).GetProperty(name)!;

        [Fact]
        public void Constructor_Should_Set_IsBool_True_For_Bool_Property()
        {
            var target = new PropertyGridTestObject();
            var prop = GetProperty(nameof(PropertyGridTestObject.IsActive));
            var model = new PropertyItemModel(prop, target, false);

            model.IsBool.Should().BeTrue();
        }

        [Fact]
        public void Constructor_Should_Set_IsEnum_True_For_Enum_Property()
        {
            var target = new PropertyGridTestObject();
            var prop = GetProperty(nameof(PropertyGridTestObject.Status));
            var model = new PropertyItemModel(prop, target, false);

            model.IsEnum.Should().BeTrue();
        }

        [Fact]
        public void Constructor_Should_Set_IsNumeric_True_For_Numeric_Types()
        {
            var target = new PropertyGridTestObject();
            var prop = GetProperty(nameof(PropertyGridTestObject.Age));
            var model = new PropertyItemModel(prop, target, false);

            model.IsNumeric.Should().BeTrue();
            model.IsInteger.Should().BeTrue();
        }

        [Fact]
        public void Constructor_Should_Set_IsDateTime_True_For_DateTime_Property()
        {
            var target = new PropertyGridTestObject();
            var prop = GetProperty(nameof(PropertyGridTestObject.Created));
            var model = new PropertyItemModel(prop, target, false);

            model.IsDateTime.Should().BeTrue();
        }

        [Fact]
        public void Constructor_Should_Set_Initial_Value_From_Target()
        {
            var target = new PropertyGridTestObject { Name = "John Doe" };
            var prop = GetProperty(nameof(PropertyGridTestObject.Name));
            var model = new PropertyItemModel(prop, target, false);

            model.Value.Should().Be("John Doe");
        }

        [Fact]
        public void Constructor_Should_Set_Description_From_DescriptionAttribute()
        {
            var target = new PropertyGridTestObject();
            var prop = GetProperty(nameof(PropertyGridTestObject.IsActive));
            var model = new PropertyItemModel(prop, target, false);

            model.Description.Should().Be("Флаг активности");
        }

        [Fact]
        public void Constructor_Should_Fallback_To_DisplayName_When_No_Description()
        {
            var target = new PropertyGridTestObject();
            var prop = GetProperty(nameof(PropertyGridTestObject.Name));
            var model = new PropertyItemModel(prop, target, false);

            model.Description.Should().Be("Name");
        }

        [Fact]
        public void Constructor_Should_Set_EnumDisplayItems_With_Enum_Names()
        {
            var target = new PropertyGridTestObject();
            var prop = GetProperty(nameof(PropertyGridTestObject.Status));
            var model = new PropertyItemModel(prop, target, false);

            model.EnumDisplayItems.Should().HaveCount(3);
            model.EnumDisplayItems!.Select(i => i.DisplayName).Should()
                .BeEquivalentTo("Active", "Inactive", "Pending");
        }

        [Fact]
        public void Constructor_Should_Set_SelectedEnumItem_From_Value()
        {
            var target = new PropertyGridTestObject { Status = TestEnum.Inactive };
            var prop = GetProperty(nameof(PropertyGridTestObject.Status));
            var model = new PropertyItemModel(prop, target, false);

            model.SelectedEnumItem.Should().NotBeNull();
            model.SelectedEnumItem!.Value.Should().Be(TestEnum.Inactive);
        }

        [Fact]
        public void Constructor_Should_Set_EnumToolTip_With_Descriptions()
        {
            var target = new PropertyGridTestObject();
            var prop = GetProperty(nameof(PropertyGridTestObject.Status));
            var model = new PropertyItemModel(prop, target, false);

            model.EnumToolTip.Should().Contain("Active: Активен")
                .And.Contain("Inactive: Неактивен")
                .And.Contain("Pending: В ожидании");
        }

        [Fact]
        public void When_Value_Changed_Should_Raise_PropertyChanged()
        {
            var target = new PropertyGridTestObject();
            var prop = GetProperty(nameof(PropertyGridTestObject.Name));
            var model = new PropertyItemModel(prop, target, false);

            var raised = false;
            model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PropertyItemModel.Value))
                    raised = true;
            };

            model.Value = "New Value";
            raised.Should().BeTrue();
        }

        [Fact]
        public void When_SelectedEnumItem_Changed_Should_Update_Value()
        {
            var target = new PropertyGridTestObject();
            var prop = GetProperty(nameof(PropertyGridTestObject.Status));
            var model = new PropertyItemModel(prop, target, false);

            var newItem = model.EnumDisplayItems!.First(i => (TestEnum)i.Value == TestEnum.Pending);
            model.SelectedEnumItem = newItem;

            model.Value.Should().Be(TestEnum.Pending);
        }

        [Fact]
        public void When_DateValue_Changed_Should_Update_Value()
        {
            var target = new PropertyGridTestObject();
            var prop = GetProperty(nameof(PropertyGridTestObject.Created));
            var model = new PropertyItemModel(prop, target, false);

            var newDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            model.DateValue = newDate;

            model.Value.Should().Be(newDate.DateTime);
        }

        [Fact]
        public void DateValue_Getter_Should_Convert_Value_Correctly()
        {
            var target = new PropertyGridTestObject { Created = new DateTime(2025, 5, 10) };
            var prop = GetProperty(nameof(PropertyGridTestObject.Created));
            var model = new PropertyItemModel(prop, target, false);

            model.DateValue.Should().Be(new DateTimeOffset(2025, 5, 10, 0, 0, 0, TimeZoneInfo.Local.GetUtcOffset(new DateTime(2025, 5, 10))));
        }

        [Fact]
        public void IsEditable_Should_Be_False_When_IsReadOnly_True()
        {
            var target = new PropertyGridTestObject();
            var prop = GetProperty(nameof(PropertyGridTestObject.Name));
            var model = new PropertyItemModel(prop, target, true);

            model.IsEditable.Should().BeFalse();
        }

        [Fact]
        public void IsEditable_Should_Be_True_When_Not_ReadOnly()
        {
            var target = new PropertyGridTestObject();
            var prop = GetProperty(nameof(PropertyGridTestObject.Name));
            var model = new PropertyItemModel(prop, target, false);

            model.IsEditable.Should().BeTrue();
        }

        [Fact]
        public void Value_Set_To_Null_For_NonNullable_ValueType_Should_Not_Throw_In_Model()
        {
            var target = new PropertyGridTestObject();
            var prop = GetProperty(nameof(PropertyGridTestObject.Age));
            var model = new PropertyItemModel(prop, target, false);

            model.Value = null;
            model.Value.Should().BeNull();
        }

        [Fact]
        public void ValidateValue_Should_Accept_Int_Value_For_Int_Property()
        {
            var target = new ValidationTestObject { Number = 0 };
            var prop = typeof(ValidationTestObject).GetProperty(nameof(ValidationTestObject.Number));
            var model = new PropertyItemModel(prop, target, false);

            model.Value = 50;

            model.ValidationError.Should().BeEmpty();

            model.Value.Should().Be(50);
        }
    }
}
