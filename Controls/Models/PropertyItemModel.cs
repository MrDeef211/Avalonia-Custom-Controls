using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Controls.Models
{
    public class PropertyItemModel : ReactiveObject
    {
        private object? _value;
        private bool _isReadOnly;
        private string _validationError = string.Empty;
        private IEnumerable<EnumItem>? _enumDisplayItems;
        private EnumItem? _selectedEnumItem;
        private readonly bool _isRequired;

        public PropertyItemModel(PropertyInfo propertyInfo, object target, bool isReadOnly)
        {
            PropertyInfo = propertyInfo;
            Target = target;
            PropertyName = propertyInfo.Name;
            _value = propertyInfo.GetValue(target);
            _isReadOnly = isReadOnly;
            _isRequired = propertyInfo.GetCustomAttribute<RequiredAttribute>() != null;
            PropertyType = propertyInfo.PropertyType;

            IsBool = PropertyType == typeof(bool);
            IsEnum = PropertyType.IsEnum;
            IsNumeric = PropertyType.IsNumericType();
            IsInteger = PropertyType.IsIntegerType();
            IsDateTime = PropertyType == typeof(DateTime) || PropertyType == typeof(DateTime?);

            var descAttr = propertyInfo.GetCustomAttribute<DescriptionAttribute>();
            var displayNameAttr = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
            Description = descAttr?.Description ?? displayNameAttr?.DisplayName ?? propertyInfo.Name;
            EnumToolTip = Description + ":\n";

            if (IsEnum)
            {
                var enumType = propertyInfo.PropertyType;
                var enumNames = Enum.GetNames(enumType);
                var toolTipLines = new List<string>();
                foreach (var name in enumNames)
                {
                    var field = enumType.GetField(name);
                    var enumDescAttr = field?.GetCustomAttribute<DescriptionAttribute>();
                    var display = enumDescAttr?.Description ?? name;
                    toolTipLines.Add($"{name}: {display}");
                }
                EnumToolTip += string.Join(Environment.NewLine, toolTipLines);

                var values = Enum.GetValues(PropertyType).Cast<object>();
                _enumDisplayItems = values
                    .Select(v => new EnumItem(v, v.ToString()))
                    .ToList();
                _selectedEnumItem = _value != null
                    ? _enumDisplayItems.FirstOrDefault(i => Equals(i.Value, _value))
                    : null;

                this.RaisePropertyChanged(nameof(EnumDisplayItems));
                this.RaisePropertyChanged(nameof(SelectedEnumItem));

                this.WhenAnyValue(x => x.SelectedEnumItem)
                    .Subscribe(item =>
                    {
                        if (item != null && !Equals(Value, item.Value))
                            Value = item.Value;
                        else if (item == null)
                            Value = null;
                    });
            }
            this.RaisePropertyChanged(nameof(DateValue));
            this.RaisePropertyChanged(nameof(Value));
        }

        public string PropertyName { get; }
        public PropertyInfo PropertyInfo { get; }
        public object Target { get; }
        public Type PropertyType { get; }
        public string Description { get; }
        public string EnumToolTip { get; }

        public object? Value
        {
            get => _value;
            set
            {
                ValidateValue(value);
                this.RaiseAndSetIfChanged(ref _value, value);
                this.RaisePropertyChanged(nameof(DateValue));
                if (IsEnum && EnumDisplayItems != null)
                {
                    SelectedEnumItem = value != null
                        ? EnumDisplayItems.FirstOrDefault(i => Equals(i.Value, value))
                        : null;
                }
            }
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                this.RaiseAndSetIfChanged(ref _isReadOnly, value);
                this.RaisePropertyChanged(nameof(IsEditable));
            }
        }

        public bool IsEditable => !IsReadOnly;

        public string ValidationError
        {
            get => _validationError;
            set => this.RaiseAndSetIfChanged(ref _validationError, value);
        }

        public bool IsBool { get; }
        public bool IsEnum { get; }
        public bool IsNumeric { get; }
        public bool IsInteger { get; }
        public bool IsDateTime { get; }

        public DateTimeOffset? DateValue
        {
            get
            {
                if (Value == null) return null;
                if (Value is DateTimeOffset dto) return dto;
                if (Value is DateTime dt)
                {
                    var offset = dt.Kind == DateTimeKind.Utc ? TimeSpan.Zero : TimeZoneInfo.Local.GetUtcOffset(dt);
                    return new DateTimeOffset(dt, offset);
                }
                if (DateTimeOffset.TryParse(Value.ToString(), out var parsed))
                    return parsed;
                return null;
            }
            set
            {
                if (value.HasValue)
                    Value = value.Value.DateTime;
                else
                    Value = null;
            }
        }

        public IEnumerable<EnumItem>? EnumDisplayItems
        {
            get => _enumDisplayItems;
            private set => this.RaiseAndSetIfChanged(ref _enumDisplayItems, value);
        }

        public EnumItem? SelectedEnumItem
        {
            get => _selectedEnumItem;
            set => this.RaiseAndSetIfChanged(ref _selectedEnumItem, value);
        }

        private string GetEnumDisplayName(object enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var descAttr = field?.GetCustomAttribute<DescriptionAttribute>();
            return descAttr?.Description ?? enumValue.ToString();
        }

        private void ValidateValue(object? value)
        {
            var targetType = Nullable.GetUnderlyingType(PropertyType) ?? PropertyType;
            object? convertedValue = null;

            if (value != null)
            {
                try
                {
                    convertedValue = Convert.ChangeType(value, targetType);
                }
                catch (Exception ex)
                {
                    ValidationError = $"Ошибка преобразования: {ex.Message}";
                    return;
                }
            }

            if (!_isRequired && (value == null || (value is string str && string.IsNullOrEmpty(str))))
            {
                ValidationError = string.Empty;
                return;
            }

            if (value == null && PropertyType.IsValueType && Nullable.GetUnderlyingType(PropertyType) == null)
            {
                ValidationError = "Значение не может быть null";
                return;
            }

            var validationContext = new ValidationContext(Target ?? new object())
            {
                MemberName = PropertyName
            };
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateProperty(convertedValue, validationContext, results))
            {
                ValidationError = results[0].ErrorMessage ?? "Некорректное значение";
                return;
            }

            ValidationError = string.Empty;
        }
    }
}
