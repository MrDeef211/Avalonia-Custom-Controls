using FluentAssertions;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using Controls.Tests.TestHelpers;
using Controls.Tests.Models;

namespace Controls.Tests.Controls
{
    public class PropertyGridControlTests
    {
        [Fact]
        public void GenerateEditors_Should_Create_Categories_From_CategoryAttribute()
        {
            var control = new PropertyGridControl();
            var obj = new PropertyGridTestObject();

            control.GenerateEditors(obj);

            control.Categories.Should().HaveCount(3);
            control.Categories.Select(c => c.Name).Should().Contain("Основные", "Дополнительно", "Общие");
        }

        [Fact]
        public void GenerateEditors_Should_Group_Properties_By_Category()
        {
            var control = new PropertyGridControl();
            var obj = new PropertyGridTestObject();

            control.GenerateEditors(obj);

            var mainCategory = control.Categories.First(c => c.Name == "Основные");
            mainCategory.Properties.Select(p => p.PropertyName).Should().Contain(nameof(PropertyGridTestObject.Name));
        }

        [Fact]
        public void GenerateEditors_Should_Set_ReadOnly_For_Properties_Without_Public_Setter()
        {
            var control = new PropertyGridControl();
            var obj = new PropertyGridTestObject();

            control.GenerateEditors(obj);

            var readOnlyProp = control.Categories
                .SelectMany(c => c.Properties)
                .First(p => p.PropertyName == nameof(PropertyGridTestObject.ReadOnlyProperty));

            readOnlyProp.IsReadOnly.Should().BeTrue();
            readOnlyProp.IsEditable.Should().BeFalse();
        }

        [Fact]
        public void GenerateEditors_Should_Set_ReadOnly_For_Private_Set_Properties()
        {
            var control = new PropertyGridControl();
            var obj = new PropertyGridTestObject();

            control.GenerateEditors(obj);

            var privateSetProp = control.Categories
                .SelectMany(c => c.Properties)
                .First(p => p.PropertyName == nameof(PropertyGridTestObject.PrivateSetProperty));

            privateSetProp.IsReadOnly.Should().BeTrue();
        }

        [Fact]
        public void GenerateEditors_Should_Set_ReadOnly_When_Control_IsReadOnly_True()
        {
            var control = new PropertyGridControl { IsReadOnly = true };
            var obj = new PropertyGridTestObject();

            control.GenerateEditors(obj);

            foreach (var prop in control.Categories.SelectMany(c => c.Properties))
            {
                prop.IsReadOnly.Should().BeTrue();
            }
        }

        [Fact]
        public void When_PropertyItemModel_Value_Changes_Should_Update_Target_Property()
        {
            var control = new PropertyGridControl();
            var obj = new PropertyGridTestObject { Name = "Old" };
            control.GenerateEditors(obj);

            var nameProp = control.Categories
                .SelectMany(c => c.Properties)
                .First(p => p.PropertyName == nameof(PropertyGridTestObject.Name));

            nameProp.Value = "New Name";

            obj.Name.Should().Be("New Name");
        }

        [Fact]
        public void When_PropertyItemModel_Value_Invalid_Should_Set_ValidationError_And_Not_Update_Target()
        {
            var control = new PropertyGridControl();
            var obj = new PropertyGridTestObject { Age = 10 };
            control.GenerateEditors(obj);

            var ageProp = control.Categories
                .SelectMany(c => c.Properties)
                .First(p => p.PropertyName == nameof(PropertyGridTestObject.Age));

            ageProp.Value = null;

            ageProp.ValidationError.Should().NotBeNullOrEmpty();
            obj.Age.Should().Be(10);
        }

        [Fact]
        public void When_Value_Changed_To_Same_Value_Should_Not_Set_HasChanges()
        {
            var control = new PropertyGridControl();
            var obj = new PropertyGridTestObject { Name = "Same" };
            control.GenerateEditors(obj);

            var nameProp = control.Categories
                .SelectMany(c => c.Properties)
                .First(p => p.PropertyName == nameof(PropertyGridTestObject.Name));

            control.HasChanges.Should().BeFalse();

            nameProp.Value = "Same"; 

            control.HasChanges.Should().BeFalse();
        }

        [Fact]
        public void When_Value_Changed_Should_Set_HasChanges_True()
        {
            var control = new PropertyGridControl();
            var obj = new PropertyGridTestObject { Name = "Old" };
            control.GenerateEditors(obj);

            var nameProp = control.Categories
                .SelectMany(c => c.Properties)
                .First(p => p.PropertyName == nameof(PropertyGridTestObject.Name));

            nameProp.Value = "Changed";

            control.HasChanges.Should().BeTrue();
        }

        [Fact]
        public void GenerateEditors_With_Null_Target_Should_Clear_Categories()
        {
            var control = new PropertyGridControl();
            var obj = new PropertyGridTestObject();
            control.GenerateEditors(obj);
            control.Categories.Should().NotBeEmpty();

            control.GenerateEditors(null);

            control.Categories.Should().BeEmpty();
        }

        [Fact]
        public void GenerateEditors_Should_Ignore_ReactiveUI_Internal_Properties()
        {
            var reactiveObj = new ReactiveTestObject();
            var control = new PropertyGridControl();

            control.GenerateEditors(reactiveObj);

            var propNames = control.Categories.SelectMany(c => c.Properties).Select(p => p.PropertyName);
            propNames.Should().NotContain("Changing", "Changed", "ThrownExceptions");
            propNames.Should().Contain(nameof(ReactiveTestObject.UserProperty));
        }

        [Fact]
        public void Validation_Should_Apply_Range_Attribute_For_Number()
        {
            var control = new PropertyGridControl();
            var obj = new ValidationTestObject();
            control.GenerateEditors(obj);

            var numberProp = control.Categories
                .SelectMany(c => c.Properties)
                .First(p => p.PropertyName == nameof(ValidationTestObject.Number));

            numberProp.Value = 150; 
            numberProp.ValidationError.Should().Contain("от 1 до 100");
            obj.Number.Should().Be(50); 

            numberProp.Value = 75; 
            numberProp.ValidationError.Should().BeEmpty();
            obj.Number.Should().Be(75);
        }

        [Fact]
        public void Validation_Should_Apply_StringLength_Attribute()
        {
            var control = new PropertyGridControl();
            var obj = new ValidationTestObject();
            control.GenerateEditors(obj);

            var stringProp = control.Categories
                .SelectMany(c => c.Properties)
                .First(p => p.PropertyName == nameof(ValidationTestObject.ShortString));

            stringProp.Value = "too long";
            stringProp.ValidationError.Should().Contain("5 символов");

            stringProp.Value = "ok";
            stringProp.ValidationError.Should().BeEmpty();
        }

        [Fact]
        public void Validation_Should_Apply_Required_Attribute()
        {
            var control = new PropertyGridControl();
            var obj = new ValidationTestObject();
            control.GenerateEditors(obj);

            var reqProp = control.Categories
                .SelectMany(c => c.Properties)
                .First(p => p.PropertyName == nameof(ValidationTestObject.RequiredString));

            reqProp.Value = null;
            reqProp.ValidationError.Should().Contain("Обязательное поле");
            obj.RequiredString.Should().Be("initial");

            reqProp.Value = "new";
            reqProp.ValidationError.Should().BeEmpty();
        }

        private class ReactiveTestObject : ReactiveObject
        {
            public string UserProperty { get; set; } = "test";
        }
    }
}
