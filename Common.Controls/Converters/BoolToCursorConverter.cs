using Avalonia.Data.Converters;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Common.Controls.Converters
{
    public class BoolToCursorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (value is true) ? new Cursor(StandardCursorType.Hand) : Cursor.Default;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
