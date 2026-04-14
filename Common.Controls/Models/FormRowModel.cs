using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Common.Controls.Models
{
    public class FormRowModel : ReactiveObject
    {
        public ObservableCollection<FormFieldModel> Fields { get; } = new();
        private bool _isHorizontal;
        public bool IsHorizontal
        {
            get => _isHorizontal;
            set => this.RaiseAndSetIfChanged(ref _isHorizontal, value);
        }
        public int RowGroup { get; set; }
    }
}
