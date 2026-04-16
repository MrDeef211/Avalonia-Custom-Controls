using Controls.Models;
using Controls.Tests.TestHelpers;
using FluentAssertions;
using ReactiveUI;
using Moq;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Text;

namespace Controls.Tests.Controls
{
    public class DataFormControlTests
    {
        [Fact]
        public void GenerateEditors_Should_Create_Sections_From_CategoryAttribute()
        {
            var control = new DataFormControl();
            var obj = new DataFormTestObject();

            control.GenerateEditors(obj);

            control.Sections.Select(s => s.Name)
                .Should().Contain("Основные", "Дополнительно", "Статус", "Общие");
        }

        [Fact]
        public void GenerateEditors_Should_Apply_CategoryOrders()
        {
            var control = new DataFormControl
            {
                FormConfig = new DataFormConfig
                {
                    CategoryOrders =
            {
                ["Дополнительно"] = 1,
                ["Основные"] = 2,
                ["Статус"] = 0,
                ["Общие"] = 3
            }
                }
            };
            var obj = new DataFormTestObject();

            control.GenerateEditors(obj);

            control.Sections.Select(s => s.Name)
                .Should().Equal("Статус", "Дополнительно", "Основные", "Общие");
        }

        [Fact]
        public void GenerateEditors_Should_Hide_NonBrowsable_Fields()
        {
            var control = new DataFormControl
            {
                FormConfig = new DataFormConfig
                {
                    Fields =
                    {
                        ["Name"] = new DataFormFieldConfig { IsBrowsable = false }
                    }
                }
            };
            var obj = new DataFormTestObject();

            control.GenerateEditors(obj);

            var allFields = control.Sections.SelectMany(s => s.Fields);
            allFields.Select(f => f.PropertyName).Should().NotContain("Name");
        }

        [Fact]
        public void GenerateEditors_Should_Group_By_RowGroup()
        {
            var control = new DataFormControl
            {
                FormConfig = new DataFormConfig
                {
                    Fields =
                    {
                        ["Name"] = new DataFormFieldConfig { RowGroup = 1 },
                        ["Age"] = new DataFormFieldConfig { RowGroup = 1 }
                    }
                }
            };
            var obj = new DataFormTestObject();

            control.GenerateEditors(obj);

            var mainSection = control.Sections.First(s => s.Name == "Основные");
            var rowWithTwoFields = mainSection.Rows.FirstOrDefault(r => r.Fields.Count == 2);
            rowWithTwoFields.Should().NotBeNull();
            rowWithTwoFields!.Fields.Select(f => f.PropertyName).Should().Contain("Name", "Age");
        }

        [Fact]
        public void GenerateEditors_Should_Set_CategoryExpanded_From_Config()
        {
            var control = new DataFormControl
            {
                FormConfig = new DataFormConfig
                {
                    CategoryExpanded = { ["Основные"] = false, ["Дополнительно"] = true }
                }
            };
            var obj = new DataFormTestObject();

            control.GenerateEditors(obj);

            control.Sections.First(s => s.Name == "Основные").IsExpanded.Should().BeFalse();
            control.Sections.First(s => s.Name == "Дополнительно").IsExpanded.Should().BeTrue();
        }

        [Fact]
        public void GenerateEditors_Should_Set_CategoryCollapsible()
        {
            var control = new DataFormControl
            {
                FormConfig = new DataFormConfig
                {
                    CategoryCollapsible = { ["Основные"] = true, ["Дополнительно"] = false }
                }
            };
            var obj = new DataFormTestObject();

            control.GenerateEditors(obj);

            control.Sections.First(s => s.Name == "Основные").CanCollapse.Should().BeTrue();
            control.Sections.First(s => s.Name == "Дополнительно").CanCollapse.Should().BeFalse();
        }

        [Fact]
        public void HasChanges_Should_Be_True_When_Field_Value_Changes()
        {
            var control = new DataFormControl();
            var obj = new DataFormTestObject { Name = "Старое" };
            control.GenerateEditors(obj);
            control.HasChanges.Should().BeFalse();

            var nameField = control.Sections.SelectMany(s => s.Fields).First(f => f.PropertyName == "Name");
            nameField.Value = "Новое";

            control.HasChanges.Should().BeTrue();
        }

        [Fact]
        public void CommitChanges_Should_Apply_Changes_To_Target()
        {
            var control = new DataFormControl();
            var obj = new DataFormTestObject { Name = "Старое" };
            control.GenerateEditors(obj);

            var nameField = control.Sections.SelectMany(s => s.Fields).First(f => f.PropertyName == "Name");
            nameField.Value = "Новое";

            control.CommitChanges();

            obj.Name.Should().Be("Новое");
            control.HasChanges.Should().BeFalse();
        }

        [Fact]
        public void CancelChanges_Should_Reset_Fields_To_Original()
        {
            var control = new DataFormControl();
            var obj = new DataFormTestObject { Name = "Исходное" };
            control.GenerateEditors(obj);

            var nameField = control.Sections.SelectMany(s => s.Fields).First(f => f.PropertyName == "Name");
            nameField.Value = "Изменённое";

            control.CancelChanges();

            nameField.Value.Should().Be("Исходное");
            control.HasChanges.Should().BeFalse();
        }

        [Fact]
        public void CommitChanges_Should_Not_Apply_Invalid_Fields()
        {
            var control = new DataFormControl();
            var obj = new DataFormTestObject { Age = 30 };
            control.GenerateEditors(obj);

            var ageField = control.Sections.SelectMany(s => s.Fields).First(f => f.PropertyName == "Age");
            ageField.Value = 15;
            ageField.ValidationError.Should().NotBeNullOrEmpty();

            control.CommitChanges();

            obj.Age.Should().Be(30); 
        }

        [Fact]
        public void SaveCommand_Should_Execute_After_Commit()
        {
            var control = new DataFormControl();
            var obj = new DataFormTestObject();
            control.GenerateEditors(obj);
            control.SelectedObject = obj;

            var mockCommand = new Mock<System.Windows.Input.ICommand>();
            control.SaveCommand = mockCommand.Object;

            var nameField = control.Sections.SelectMany(s => s.Fields).First(f => f.PropertyName == "Name");
            nameField.Value = "Новое";
            control.CommitChanges();

            mockCommand.Verify(cmd => cmd.Execute(obj), Times.Once);
        }

        [Fact]
        public void CancelCommand_Should_Execute_After_Cancel()
        {
            var control = new DataFormControl();
            var obj = new DataFormTestObject();
            control.GenerateEditors(obj);
            control.SelectedObject = obj;

            var mockCommand = new Mock<System.Windows.Input.ICommand>();
            control.CancelCommand = mockCommand.Object;

            control.CancelChanges();

            mockCommand.Verify(cmd => cmd.Execute(obj), Times.Once);
        }

        [Fact]
        public void ReadOnly_Properties_Should_Not_Be_Editable()
        {
            var control = new DataFormControl();
            var obj = new DataFormTestObject();

            control.GenerateEditors(obj);

            var readOnlyField = control.Sections.SelectMany(s => s.Fields).First(f => f.PropertyName == "ReadOnlyProp");
            readOnlyField.IsReadOnly.Should().BeTrue();
            readOnlyField.IsEditable.Should().BeFalse();
        }

        [Fact]
        public void Validation_Should_Apply_Required_Attribute_In_DataForm()
        {
            var control = new DataFormControl();
            var obj = new DataFormTestObject();
            control.GenerateEditors(obj);

            var field = control.Sections.SelectMany(s => s.Fields)
                .First(f => f.PropertyName == nameof(DataFormTestObject.RequiredName));

            field.Value = null;
            field.ValidationError.Should().Contain("Имя обязательно");
            control.HasChanges.Should().BeTrue(); 

            control.CommitChanges();
            obj.RequiredName.Should().Be("John"); 
        }

        [Fact]
        public void Validation_Should_Use_CustomErrorMessage_From_Config()
        {
            var control = new DataFormControl
            {
                FormConfig = new DataFormConfig
                {
                    Fields =
            {
                ["Age"] = new DataFormFieldConfig
                {
                    Validation = new DataFormFieldValidation
                    {
                        Min = 18,
                        CustomErrorMessage = "Слишком молод"
                    }
                }
            }
                }
            };
            var obj = new DataFormTestObject { Age = 30 };
            control.GenerateEditors(obj);

            var ageField = control.Sections.SelectMany(s => s.Fields).First(f => f.PropertyName == "Age");
            ageField.Value = 10;
            ageField.ValidationError.Should().Be("Слишком молод");
        }
    }
}
