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
            set => this.RaiseAndSetIfChanged(ref _value, value);
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
