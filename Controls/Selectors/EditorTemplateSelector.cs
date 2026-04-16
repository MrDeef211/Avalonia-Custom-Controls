using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Controls.Models;

namespace Controls.Selectors
{
    public class EditorTemplateSelector : IDataTemplate
    {
        public Control Build(object? param)
        {
            if (param is FormFieldModel model)
            {
                if (model.IsBool)
                {
                    var checkBox = new CheckBox
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Avalonia.Thickness(0)
                    };
                    checkBox.Bind(CheckBox.IsCheckedProperty, new Binding("Value") { Mode = BindingMode.TwoWay });
                    checkBox.Bind(CheckBox.IsEnabledProperty, new Binding("IsEditable"));
                    return checkBox;
                }

                Control editor;

                if (model.IsEnum)
                {
                    var combo = new ComboBox
                    {
                        Height = 24,
                        FontSize = 12,
                        Padding = new Avalonia.Thickness(4, 2),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    combo.ItemsSource = model.EnumDisplayItems;
                    combo.DisplayMemberBinding = new Binding("DisplayName");
                    combo.Bind(ComboBox.SelectedItemProperty, new Binding("SelectedEnumItem") { Mode = BindingMode.TwoWay });
                    combo.Bind(ComboBox.IsEnabledProperty, new Binding("IsEditable"));
                    editor = combo;
                }
                else if (model.IsNumeric)
                {
                    var numeric = new NumericUpDown
                    {
                        Height = 24,
                        FontSize = 12,
                        Padding = new Avalonia.Thickness(4, 2),
                        VerticalAlignment = VerticalAlignment.Center
                    };
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
                    editor = numeric;
                }
                else if (model.IsDateTime)
                {
                    var picker = new DatePicker
                    {
                        Height = 24,
                        FontSize = 12,
                        Padding = new Avalonia.Thickness(4, 2),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    picker.Bind(DatePicker.SelectedDateProperty, new Binding("DateValue") { Mode = BindingMode.TwoWay });
                    picker.Bind(DatePicker.IsEnabledProperty, new Binding("IsEditable"));
                    editor = picker;
                }
                else
                {
                    var textBox = new TextBox
                    {
                        Height = 24,
                        FontSize = 12,
                        Padding = new Avalonia.Thickness(4, 2),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    textBox.Bind(TextBox.TextProperty, new Binding("Value") { Mode = BindingMode.TwoWay });
                    textBox.Bind(TextBox.IsEnabledProperty, new Binding("IsEditable"));
                    editor = textBox;
                }

                return new ScrollViewer
                {
                    Content = editor,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
            }

            return new TextBox(); // fallback
        }

        public bool Match(object? data) => data is FormFieldModel;
    }
}