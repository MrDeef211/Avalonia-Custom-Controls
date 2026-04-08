using ReactiveUI;
using System;
using System.Collections.Generic;
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

        public FormFieldModel(PropertyInfo propertyInfo, object target, bool isReadOnly)
        {
            PropertyInfo = propertyInfo;
            Target = target;
            PropertyName = propertyInfo.Name;
            _originalValue = propertyInfo.GetValue(target);
            _value = _originalValue;
            _isReadOnly = isReadOnly;
        }

        public string PropertyName { get; }
        public PropertyInfo PropertyInfo { get; }
        public object Target { get; }

        public object? Value
        {
            get => _value;
            set 
            { 
                this.RaiseAndSetIfChanged(ref _value, value);
                TryConvertValue();
            }
        }

        public object? ConvertedValue
        {
            get => _convertedValue;
            private set => this.RaiseAndSetIfChanged(ref _convertedValue, value);
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

        private void TryConvertValue()
        {
            ValidationError = string.Empty;

            if (_value == null)
            {
                var propType = PropertyInfo.PropertyType;
                if (propType.IsValueType && Nullable.GetUnderlyingType(propType) == null)
                {
                    ValidationError = "Значение не может быть null";
                    ConvertedValue = null;
                    return;
                }
                ConvertedValue = null;
                return;
            }

            var targetType = Nullable.GetUnderlyingType(PropertyInfo.PropertyType) ?? PropertyInfo.PropertyType;
            try
            {
                var converted = Convert.ChangeType(_value, targetType);
                ConvertedValue = converted;
            }
            catch (Exception ex)
            {
                ValidationError = $"Ошибка: {ex.Message}";
                ConvertedValue = null;
            }
        }

        /// <summary>
        /// Сбросить буферное значение до исходного (отмена).
        /// </summary>
        public void ResetToOriginal()
        {
            Value = _originalValue;
            ValidationError = string.Empty;
        }

        /// <summary>
        /// Обновить исходное значение текущим буферным (после сохранения).
        /// </summary>
        public void UpdateOriginal()
        {
            _originalValue = _value;
            this.RaisePropertyChanged(nameof(OriginalValue));
        }
    }
}
