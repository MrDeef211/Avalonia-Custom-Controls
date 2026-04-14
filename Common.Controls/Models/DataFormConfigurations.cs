using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Controls.Models
{
    public enum FieldLayout
    {
        // поле в отдельной строке
        Vertical,   

        // поле в одной строке с предыдущим (группировка)
        Horizontal
    }

    public class DataFormFieldValidation
    {
        public double? Min { get; set; }
        public double? Max { get; set; }
        public int? MaxLength { get; set; }
        public string? RegexPattern { get; set; }
        public string? CustomErrorMessage { get; set; }
    }

    public class DataFormFieldConfig
    {
        public string PropertyName { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Category { get; set; }
        public bool? IsReadOnly { get; set; }
        public bool IsBrowsable { get; set; } = true;
        public bool IsCategoryExpanded { get; set; } = true;
        public int Order { get; set; } = 0;
        public int? CategoryOrder { get; set; }
        public bool HideLabel { get; set; } = false;
        public FieldLayout Layout { get; set; } = FieldLayout.Vertical;
        public int RowGroup { get; set; } = -1;
        public DataFormFieldValidation? Validation { get; set; }
    }

    public class DataFormConfig
    {
        public Dictionary<string, bool> CategoryCollapsible { get; set; } = new();
        public Dictionary<string, DataFormFieldConfig> Fields { get; set; } = new();
        public Dictionary<string, int> CategoryOrders { get; set; } = new();
        public void SetCategoryOrder(string categoryName, int order)
        {
            var normalized = categoryName?.Trim();
            if (string.IsNullOrEmpty(normalized))
                throw new ArgumentException("Имя категории не может быть пустым");
            CategoryOrders[normalized] = order;
        }
    }
}
