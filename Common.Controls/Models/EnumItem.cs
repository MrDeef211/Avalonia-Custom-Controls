using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Controls.Models
{
    public class EnumItem
    {
        public object Value { get; set; }
        public string DisplayName { get; set; }

        public EnumItem(object value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }
    }
}
