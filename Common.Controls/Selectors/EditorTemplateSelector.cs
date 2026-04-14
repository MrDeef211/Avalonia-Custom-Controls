using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Common.Controls.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Controls.Selectors
{
    public class EditorTemplateSelector : IDataTemplate
    {
        public Control Build(object param)
        {
            if (param is FormFieldModel model)
            {
                if (model.IsBool)
                {
                    var checkBox = new CheckBox();
                    checkBox.Bind(CheckBox.IsCheckedProperty, new Binding("Value") { Mode = BindingMode.TwoWay });
                    checkBox.Bind(CheckBox.IsEnabledProperty, new Binding("IsEditable"));
                    return checkBox;
                }
                if (model.IsEnum)
                {
                    var combo = new ComboBox();
                    combo.ItemsSource = model.EnumDisplayItems;
                    combo.DisplayMemberBinding = new Binding("DisplayName");
                    combo.Bind(ComboBox.SelectedItemProperty, new Binding("SelectedEnumItem") { Mode = BindingMode.TwoWay });
                    combo.Bind(ComboBox.IsEnabledProperty, new Binding("IsEditable"));
                    return combo;
                }
                if (model.IsNumeric)
                {
                    var numeric = new NumericUpDown();
                    numeric.Bind(NumericUpDown.ValueProperty, new Binding("Value") { Mode = BindingMode.TwoWay });
                    numeric.Bind(NumericUpDown.IsEnabledProperty, new Binding("IsEditable"));

                    if (model.IsInteger)
                    {
                        numeric.Increment = 1;
                        numeric.FormatString = "F0";
                    }
                    else
                    {
                        numeric.Increment = 0.1m;
                    }
                    return numeric;
                }
                if (model.IsDateTime)
                {
                    var picker = new DatePicker();
                    picker.Bind(DatePicker.SelectedDateProperty, new Binding("DateValue") { Mode = BindingMode.TwoWay });
                    picker.Bind(DatePicker.IsEnabledProperty, new Binding("IsEditable"));
                    return picker;
                }
            }
            var textBox = new TextBox();
            textBox.Bind(TextBox.TextProperty, new Binding("Value") { Mode = BindingMode.TwoWay });
            textBox.Bind(TextBox.IsEnabledProperty, new Binding("IsEditable"));
            return textBox;
        }

        public bool Match(object data) => data is FormFieldModel;
    }
}

