using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Controls.Models;

namespace Controls.Selectors
{
    public class PropertyEditorTemplateSelector : IDataTemplate
    {
        public Control Build(object? param)
        {
            if (param is not PropertyItemModel model)
                return new TextBox();

            if (model.IsBool)
            {
                var checkBox = new CheckBox
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Avalonia.Thickness(0)
                };
                checkBox.Classes.Add("PropertyGridEditor");
                checkBox.Bind(CheckBox.IsCheckedProperty, new Binding("Value") { Mode = BindingMode.TwoWay });
                checkBox.Bind(CheckBox.IsEnabledProperty, new Binding("IsEditable"));
                checkBox.Bind(ToolTip.TipProperty, new Binding("Description"));
                return checkBox;
            }

            if (model.IsEnum)
            {
                var combo = new ComboBox
                {
                    Height = 24,
                    FontSize = 12,
                    FontWeight = Avalonia.Media.FontWeight.Normal,
                    FontStyle = Avalonia.Media.FontStyle.Normal,
                    Padding = new Avalonia.Thickness(4, 2),
                    VerticalAlignment = VerticalAlignment.Center
                };
                combo.Classes.Add("PropertyGridEditor");
                combo.ItemsSource = model.EnumDisplayItems;
                combo.DisplayMemberBinding = new Binding("DisplayName");
                combo.Bind(ComboBox.SelectedItemProperty, new Binding("SelectedEnumItem") { Mode = BindingMode.TwoWay });
                combo.Bind(ComboBox.IsEnabledProperty, new Binding("IsEditable"));
                combo.Bind(ToolTip.TipProperty, new Binding("EnumToolTip"));
                return combo;
            }

            if (model.IsNumeric)
            {
                var numeric = new NumericUpDown
                {
                    Height = 24,
                    FontSize = 12,
                    FontWeight = Avalonia.Media.FontWeight.Normal,
                    FontStyle = Avalonia.Media.FontStyle.Normal,
                    Padding = new Avalonia.Thickness(4, 2),
                    VerticalAlignment = VerticalAlignment.Center
                };
                numeric.Classes.Add("PropertyGridEditor");
                numeric.Bind(NumericUpDown.ValueProperty, new Binding("Value") { Mode = BindingMode.TwoWay });
                numeric.Bind(NumericUpDown.IsEnabledProperty, new Binding("IsEditable"));
                numeric.Bind(ToolTip.TipProperty, new Binding("Description"));

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
                var picker = new DatePicker
                {
                    Height = 24,
                    FontSize = 12,
                    FontWeight = Avalonia.Media.FontWeight.Normal,
                    FontStyle = Avalonia.Media.FontStyle.Normal,
                    Padding = new Avalonia.Thickness(4, 2),
                    VerticalAlignment = VerticalAlignment.Center
                };
                picker.Classes.Add("PropertyGridEditor");
                picker.Bind(DatePicker.SelectedDateProperty, new Binding("DateValue") { Mode = BindingMode.TwoWay });
                picker.Bind(DatePicker.IsEnabledProperty, new Binding("IsEditable"));
                picker.Bind(ToolTip.TipProperty, new Binding("Description"));
                return picker;
            }

            var textBox = new TextBox
            {
                Height = 24,
                FontSize = 12,
                FontWeight = Avalonia.Media.FontWeight.Normal,
                FontStyle = Avalonia.Media.FontStyle.Normal,
                Padding = new Avalonia.Thickness(4, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            textBox.Classes.Add("PropertyGridEditor");
            textBox.Bind(TextBox.TextProperty, new Binding("Value") { Mode = BindingMode.TwoWay });
            textBox.Bind(TextBox.IsEnabledProperty, new Binding("IsEditable"));
            textBox.Bind(ToolTip.TipProperty, new Binding("Description"));
            return textBox;
        }

        public bool Match(object? data) => data is PropertyItemModel;
    }
}