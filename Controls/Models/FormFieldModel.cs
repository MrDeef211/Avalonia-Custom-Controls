using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Controls.Models
{
    public class FormFieldModel : ReactiveObject
    {
        private object? _value;
        private bool _isReadOnly;
        private string _validationError = string.Empty;
        private bool _isTouched;
        private int _rowGroup = -1;
        private IEnumerable<EnumItem>? _enumDisplayItems;

        public FormFieldModel(PropertyInfo propertyInfo, object target, bool isReadOnly, DataFormFieldConfig? fieldConfig = null)
        {
            PropertyInfo = propertyInfo;
            Target = target;
            PropertyName = propertyInfo.Name;
            var displayNameAttr = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
            DisplayName = displayNameAttr?.DisplayName ?? PropertyName;
            _value = propertyInfo.GetValue(target);
            OriginalValue = _value;
            _isReadOnly = isReadOnly;
            PropertyType = propertyInfo.PropertyType;

            if (PropertyType.IsEnum)
            {
                var values = Enum.GetValues(PropertyType).Cast<object>();
                EnumDisplayItems = values
                    .Select(v => new EnumItem(v, GetEnumDisplayName(v)))
                    .ToList();
            }

            if (PropertyType.IsEnum)
            {
                var values = Enum.GetValues(PropertyType).Cast<object>();
                EnumDisplayItems = values
                    .Select(v => new EnumItem(v, GetEnumDisplayName(v)))
                    .ToList();

                _selectedEnumItem = _value != null
                    ? EnumDisplayItems.FirstOrDefault(i => Equals(i.Value, _value))
                    : null;
            }

            ApplyConfiguration(fieldConfig);

            this.WhenAnyValue(x => x.Value)
                .Subscribe(_ => UpdateConvertedValueAndValidation())
                .DisposeWith(Disposables);

            UpdateConvertedValueAndValidation();
        }

        private readonly CompositeDisposable Disposables = new();

        public string PropertyName { get; }
        public string DisplayName { get; private set; }
        public PropertyInfo PropertyInfo { get; }
        public object Target { get; }
        public Type PropertyType { get; }
        public object? OriginalValue { get; private set; }

        public object? Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value)) return;
                _value = value;
                IsTouched = true;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(DateValue));
            }
        }

        private object? _convertedValue;
        public object? ConvertedValue
        {
            get => _convertedValue;
            private set => this.RaiseAndSetIfChanged(ref _convertedValue, value);
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

        public int RowGroup
        {
            get => _rowGroup;
            set => this.RaiseAndSetIfChanged(ref _rowGroup, value);
        }

        public bool IsEditable => !IsReadOnly;

        public string ValidationError
        {
            get => _validationError;
            set => this.RaiseAndSetIfChanged(ref _validationError, value);
        }

        public bool IsTouched
        {
            get => _isTouched;
            private set => this.RaiseAndSetIfChanged(ref _isTouched, value);
        }

        public DateTimeOffset? DateValue
        {
            get
            {
                if (Value == null) return null;

                if (Value is DateTimeOffset dto)
                    return dto;

                if (Value is DateTime dt)
                {
                    var offset = dt.Kind == DateTimeKind.Utc ? TimeSpan.Zero : TimeZoneInfo.Local.GetUtcOffset(dt);
                    return new DateTimeOffset(dt, offset);
                }

                if (DateTimeOffset.TryParse(Value.ToString(), out var parsedDto))
                    return parsedDto;

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

        public bool IsEnum => PropertyType.IsEnum;
        public bool IsBool => PropertyType == typeof(bool);
        public bool ShowLabel => !HideLabel;
        public bool HideLabel { get; private set; }

        public int EditorColumn => ShowLabel ? 1 : 0;
        public int EditorColumnSpan => ShowLabel ? 1 : 2;

        public bool IsNumeric => PropertyType.IsNumericType();
        public bool IsInteger => PropertyType.IsIntegerType();
        public bool IsDateTime => PropertyType == typeof(DateTime) || PropertyType == typeof(DateTime?);

        public IEnumerable<EnumItem>? EnumDisplayItems
        {
            get => _enumDisplayItems;
            private set => this.RaiseAndSetIfChanged(ref _enumDisplayItems, value);
        }

        private EnumItem? _selectedEnumItem;
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
                else if (value == null)
                {
                    Value = null;
                }
            }
        }

        public DataFormFieldValidation? ValidationConfig { get; private set; }

        public void ResetToOriginal()
        {
            Value = OriginalValue;
            ValidationError = string.Empty;
            IsTouched = false;
        }

        public void UpdateOriginal()
        {
            OriginalValue = Value;
        }

        private void UpdateConvertedValueAndValidation()
        {
            var (converted, error) = TryConvertValue(Value);
            ConvertedValue = converted;
            ValidationError = error;

            if (IsEnum && EnumDisplayItems != null)
            {
                SelectedEnumItem = Value != null
                    ? EnumDisplayItems.FirstOrDefault(i => Equals(i.Value, Value))
                    : null;
            }
        }

        private (object? converted, string error) TryConvertValue(object? input)
        {
            try
            {
                if (input == null)
                {
                    if (PropertyType.IsValueType && Nullable.GetUnderlyingType(PropertyType) == null)
                        throw new InvalidOperationException("Значение не может быть null для типа значения");
                    return (null, string.Empty);
                }

                var targetType = Nullable.GetUnderlyingType(PropertyType) ?? PropertyType;
                var inputType = input.GetType();

                if (inputType == targetType)
                {
                    var validationError = Validate(input);
                    return (input, validationError);
                }

                object converted;

                if (targetType == typeof(DateTime))
                {
                    converted = ConvertToDateTime(input);
                }
                else if (targetType == typeof(DateTime?))
                {
                    converted = input == null ? null : ConvertToDateTime(input);
                }
                else
                {
                    converted = Convert.ChangeType(input, targetType);
                }

                var error = Validate(converted);
                return (converted, error);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        private static DateTime ConvertToDateTime(object value)
        {
            return value switch
            {
                DateTime dt => dt,
                DateTimeOffset dto => dto.DateTime,
                string s => DateTime.Parse(s),
                _ => (DateTime)Convert.ChangeType(value, typeof(DateTime))
            };
        }

        private string Validate(object? value)
        {
            var validationContext = new ValidationContext(Target ?? new object())
            {
                MemberName = PropertyName
            };
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateProperty(value, validationContext, results))
            {
                return results[0].ErrorMessage ?? "Некорректное значение";
            }

            if (ValidationConfig != null)
            {
                if (value == null) return string.Empty;

                if (IsNumeric && double.TryParse(value.ToString(), out var num))
                {
                    if (ValidationConfig.Min.HasValue && num < ValidationConfig.Min.Value)
                        return ValidationConfig.CustomErrorMessage ?? $"Значение не может быть меньше {ValidationConfig.Min.Value}";
                    if (ValidationConfig.Max.HasValue && num > ValidationConfig.Max.Value)
                        return ValidationConfig.CustomErrorMessage ?? $"Значение не может быть больше {ValidationConfig.Max.Value}";
                }
                else if (value is string str)
                {
                    if (ValidationConfig.MaxLength.HasValue && str.Length > ValidationConfig.MaxLength.Value)
                        return ValidationConfig.CustomErrorMessage ?? $"Максимальная длина {ValidationConfig.MaxLength.Value} символов";
                    if (!string.IsNullOrEmpty(ValidationConfig.RegexPattern) && !Regex.IsMatch(str, ValidationConfig.RegexPattern))
                        return ValidationConfig.CustomErrorMessage ?? "Некорректный формат";
                }
            }

            return string.Empty;
        }

        private void ApplyConfiguration(DataFormFieldConfig? config)
        {
            if (config != null)
            {
                if (!string.IsNullOrEmpty(config.DisplayName))
                    DisplayName = config.DisplayName;
                if (config.IsReadOnly.HasValue)
                    IsReadOnly = config.IsReadOnly.Value;
                HideLabel = config.HideLabel;
                RowGroup = config.RowGroup;
                ValidationConfig = config.Validation;
            }

        }

        private string GetEnumDisplayName(object enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var descAttr = field?.GetCustomAttribute<DescriptionAttribute>();
            return descAttr?.Description ?? enumValue.ToString();
        }


        public void Dispose()
        {
            Disposables.Dispose();
        }
    }

    internal static class TypeExtensions
    {
        public static bool IsNumericType(this Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type == typeof(int) || type == typeof(double) || type == typeof(float) ||
                   type == typeof(decimal) || type == typeof(byte) || type == typeof(short) ||
                   type == typeof(uint) || type == typeof(ushort) || type == typeof(long) ||
                   type == typeof(ulong);
        }

        public static bool IsIntegerType(this Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type == typeof(int) || type == typeof(long) || type == typeof(short) ||
                   type == typeof(byte) || type == typeof(uint) || type == typeof(ushort) ||
                   type == typeof(ulong);
        }
    }
}
