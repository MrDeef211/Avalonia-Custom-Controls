using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Common.Controls.Models
{
    public class FormFieldModel : ReactiveObject
    {
        private object? _originalValue;
        private object? _value;
        private object? _convertedValue;
        private bool _isReadOnly;
        private string _validationError = string.Empty;
        private bool _isTouched = false;
        private DateTimeOffset? _dateValue;
        private EnumItem? _selectedEnumItem;
        private IEnumerable<EnumItem>? _enumDisplayItems;
        public bool HideLabel { get; set; }
        public DataFormFieldValidation? ValidationConfig { get; set; }
        public FieldLayout Layout { get; set; }
        public int RowGroup { get; set; }

        public FormFieldModel(PropertyInfo propertyInfo, object target, bool isReadOnly, DataFormFieldConfig? fieldConfig = null)
        {
            PropertyInfo = propertyInfo;
            Target = target;
            PropertyName = propertyInfo.Name;
            var displayNameAttr = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
            DisplayName = displayNameAttr?.DisplayName ?? PropertyName;
            _originalValue = propertyInfo.GetValue(target);
            _value = _originalValue;
            _isReadOnly = isReadOnly;
            PropertyType = propertyInfo.PropertyType;

            if (PropertyType.IsEnum)
            {
                EnumValues = Enum.GetValues(PropertyType).Cast<object>().ToList();
                EnumDisplayItems = EnumValues
                    .Select(v => new EnumItem(v, GetEnumDisplayName(v)))
                    .ToList();

                if (_value != null)
                    SelectedEnumItem = EnumDisplayItems.FirstOrDefault(i => Equals(i.Value, _value));
            }

            if (PropertyType == typeof(DateTime) || PropertyType == typeof(DateTime?))
            {
                if (_value is DateTime dt)
                    _dateValue = new DateTimeOffset(dt);
                else
                {
                    var dtn = _value as DateTime?;
                    _dateValue = dtn.HasValue ? new DateTimeOffset(dtn.Value) : (DateTimeOffset?)null;
                }
            }

            if (fieldConfig != null)
            {
                if (!string.IsNullOrEmpty(fieldConfig.DisplayName))
                    DisplayName = fieldConfig.DisplayName;
                if (fieldConfig.IsReadOnly.HasValue)
                    IsReadOnly = fieldConfig.IsReadOnly.Value;
                HideLabel = fieldConfig.HideLabel;
                Layout = fieldConfig.Layout;
                RowGroup = fieldConfig.RowGroup;
                ValidationConfig = fieldConfig.Validation;
            }
        }

        public string PropertyName { get; }
        public string DisplayName { get; }
        public PropertyInfo PropertyInfo { get; }
        public object Target { get; }
        public Type PropertyType { get; }

        public object? Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value)) return;
                _value = value;
                IsTouched = true;
                this.RaisePropertyChanged();

                if (EnumDisplayItems != null && value != null)
                {
                    SelectedEnumItem = EnumDisplayItems.FirstOrDefault(i => Equals(i.Value, value));
                }
                else if (value == null)
                    SelectedEnumItem = null;

                TryConvert();
            }
        }

        public object? ConvertedValue
        {
            get => _convertedValue;
            private set => this.RaiseAndSetIfChanged(ref _convertedValue, value);
        }

        public DateTimeOffset? DateValue
        {
            get => _dateValue;
            set
            {
                this.RaiseAndSetIfChanged(ref _dateValue, value);
                if (value.HasValue)
                    Value = value.Value.DateTime;
                else
                    Value = null;
            }
        }

        public object? OriginalValue => _originalValue;

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

        public bool IsEnum => PropertyType.IsEnum;

        public bool IsBool => PropertyType == typeof(bool);

        public bool ShowLabel => !HideLabel;

        public bool IsTouched
        {
            get => _isTouched;
            private set => this.RaiseAndSetIfChanged(ref _isTouched, value);
        }

        public bool IsNumeric => PropertyType == typeof(int) || PropertyType == typeof(double) ||
                         PropertyType == typeof(float) || PropertyType == typeof(decimal) ||
                         PropertyType == typeof(byte) || PropertyType == typeof(short) ||
                         PropertyType == typeof(uint) || PropertyType == typeof(ushort) ||
                         PropertyType == typeof(long) || PropertyType == typeof(ulong);

        public bool IsInteger
        {
            get
            {
                var type = Nullable.GetUnderlyingType(PropertyType) ?? PropertyType;
                return type == typeof(int) || type == typeof(long) || type == typeof(short) ||
                       type == typeof(byte) || type == typeof(uint) || type == typeof(ushort) ||
                       type == typeof(ulong);
            }
        }

        public bool IsDateTime => PropertyType == typeof(DateTime) || PropertyType == typeof(DateTime?);

        public IEnumerable<object>? EnumValues { get; }

        public IEnumerable<EnumItem>? EnumDisplayItems
        {
            get => _enumDisplayItems;
            private set => this.RaiseAndSetIfChanged(ref _enumDisplayItems, value);
        }

        public EnumItem? SelectedEnumItem
        {
            get => _selectedEnumItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedEnumItem, value);
                if (value != null && !Equals(Value, value.Value))
                {
                    Value = value.Value;
                }
            }
        }

        public void ResetToOriginal()
        {
            Value = _originalValue;
            if (IsDateTime)
            {
                if (_originalValue is DateTime dt)
                    DateValue = new DateTimeOffset(dt);
                else
                {
                    var dtn = _originalValue as DateTime?;
                    DateValue = dtn.HasValue ? new DateTimeOffset(dtn.Value) : (DateTimeOffset?)null;
                }
            }
            ValidationError = string.Empty;
            IsTouched = false;
        }

        public void UpdateOriginal()
        {
            _originalValue = _value;
            if (IsDateTime)
            {
                if (_value is DateTime dt)
                    DateValue = new DateTimeOffset(dt);
                else
                {
                    var dtn = _value as DateTime?;
                    DateValue = dtn.HasValue ? new DateTimeOffset(dtn.Value) : (DateTimeOffset?)null;
                }
            }
        }

        private string GetEnumDisplayName(object enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var descAttr = field?.GetCustomAttribute<DescriptionAttribute>();
            return descAttr?.Description ?? enumValue.ToString();
        }

        public bool TryConvert()
        {
            try
            {
                if (Value is EnumItem enumItem)
                {
                    Value = enumItem.Value;
                }

                if (Value == null)
                {
                    var underlying = Nullable.GetUnderlyingType(PropertyType);
                    if (PropertyType.IsValueType && underlying == null)
                        throw new Exception("Значение не может быть null");
                    ConvertedValue = null;
                    ValidationError = string.Empty;
                    return true;
                }

                var targetType = Nullable.GetUnderlyingType(PropertyType) ?? PropertyType;
                var valueType = Value.GetType();

                if (valueType == targetType)
                {
                    ConvertedValue = Value;
                    ValidationError = string.Empty;
                    return true;
                }


                if (targetType == typeof(DateTime))
                {
                    if (valueType == typeof(DateTime))
                        ConvertedValue = Value;
                    else if (valueType == typeof(DateTime?))
                        ConvertedValue = ((DateTime?)Value).Value;
                    else if (valueType == typeof(DateTimeOffset))
                        ConvertedValue = ((DateTimeOffset)Value).DateTime;
                    else if (valueType == typeof(DateTimeOffset?))
                        ConvertedValue = ((DateTimeOffset?)Value)?.DateTime;
                    else if (valueType == typeof(string))
                        ConvertedValue = DateTime.Parse((string)Value);
                    else
                        ConvertedValue = Convert.ChangeType(Value, targetType);
                    ValidationError = string.Empty;
                    return true;
                }

                if (targetType == typeof(DateTime?))
                {
                    if (valueType == typeof(DateTime))
                        ConvertedValue = (DateTime?)Value;
                    else if (valueType == typeof(DateTime?))
                        ConvertedValue = Value;
                    else if (valueType == typeof(DateTimeOffset))
                        ConvertedValue = ((DateTimeOffset)Value).DateTime;
                    else if (valueType == typeof(DateTimeOffset?))
                        ConvertedValue = ((DateTimeOffset?)Value)?.DateTime;
                    else if (valueType == typeof(string))
                        ConvertedValue = DateTime.Parse((string)Value);
                    else
                        ConvertedValue = Convert.ChangeType(Value, targetType);
                    ValidationError = string.Empty;
                    return true;
                }

                ConvertedValue = Convert.ChangeType(Value, targetType);
                if (!ValidateValue())
                    return false;
                ValidationError = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                ValidationError = ex.Message;
                ConvertedValue = null;
                return false;
            }
        }

        private bool ValidateValue()
        {
            var config = ValidationConfig;
            if (config == null) return true;

            if (Value == null) return true;

            if (IsNumeric)
            {
                double numericValue = Convert.ToDouble(Value);
                if (config.Min.HasValue && numericValue < config.Min.Value)
                {
                    ValidationError = config.CustomErrorMessage ?? $"Значение не может быть меньше {config.Min.Value}";
                    return false;
                }
                if (config.Max.HasValue && numericValue > config.Max.Value)
                {
                    ValidationError = config.CustomErrorMessage ?? $"Значение не может быть больше {config.Max.Value}";
                    return false;
                }
            }
            else if (Value is string str)
            {
                if (config.MaxLength.HasValue && str.Length > config.MaxLength.Value)
                {
                    ValidationError = config.CustomErrorMessage ?? $"Максимальная длина {config.MaxLength.Value} символов";
                    return false;
                }
                if (!string.IsNullOrEmpty(config.RegexPattern) && !System.Text.RegularExpressions.Regex.IsMatch(str, config.RegexPattern))
                {
                    ValidationError = config.CustomErrorMessage ?? "Некорректный формат";
                    return false;
                }
            }
            return true;
        }
    }
}
