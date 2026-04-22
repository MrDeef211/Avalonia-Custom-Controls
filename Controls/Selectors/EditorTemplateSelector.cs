using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Styling;
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
                        Margin = new Avalonia.Thickness(0),
                        VerticalAlignment = VerticalAlignment.Center,
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
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    combo.ItemsSource = model.EnumDisplayItems;
                    combo.DisplayMemberBinding = new Binding("DisplayName");
                    combo.Bind(ComboBox.SelectedItemProperty, new Binding("SelectedEnumItem") { Mode = BindingMode.TwoWay });
                    combo.Bind(ComboBox.IsEnabledProperty, new Binding("IsEditable"));
                    editor = combo;
                    editor.Classes.Add("DataFormEditor");
                }
                else if (model.IsNumeric)
                {
                    var numeric = new NumericUpDown
                    {
                        Height = 24,
                        FontSize = 12,
                        Padding = new Avalonia.Thickness(4, 2),
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
                    editor.Classes.Add("DataFormEditor");
                }
                else if (model.IsDateTime)
                {
                    var picker = new DatePicker
                    {
                        Height = 24,
                        FontSize = 12,
                        Padding = new Avalonia.Thickness(4, 2),
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    picker.Bind(DatePicker.SelectedDateProperty, new Binding("DateValue") { Mode = BindingMode.TwoWay });
                    picker.Bind(DatePicker.IsEnabledProperty, new Binding("IsEditable"));
                    editor = picker;
                    editor.Classes.Add("DataFormEditor");
                }
                else
                {
                    var textBox = new TextBox
                    {
                        Height = 24,
                        FontSize = 12,
                        Padding = new Avalonia.Thickness(4, 2),
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    textBox.Bind(TextBox.TextProperty, new Binding("Value") { Mode = BindingMode.TwoWay });
                    textBox.Bind(TextBox.IsEnabledProperty, new Binding("IsEditable"));
                    editor = textBox;
                    editor.Classes.Add("DataFormEditor");
                }

                return new ScrollViewer
                {
                    Content = editor,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(0),                  
                    Margin = new Thickness(0),
                    Classes = { "DataFormEditorScroll" }
                };
            }

            return new TextBox(); 
        }

        public bool Match(object? data) => data is FormFieldModel;
    }
}