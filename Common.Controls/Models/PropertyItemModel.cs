using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Common.Controls.Models
{
    public class PropertyItemModel : ReactiveObject
    {
        private object? _value;
        private bool _isReadOnly;
        private string _validationError = string.Empty;

        public PropertyItemModel(PropertyInfo propertyInfo, object target, bool isReadOnly)
        {
            PropertyInfo = propertyInfo;
            Target = target;
            PropertyName = propertyInfo.Name;
            _value = propertyInfo.GetValue(target);
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
    }
}
