using Avalonia.Controls;
using Avalonia.Layout;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Controls.Models
{
    /// <summary>
    /// Модель строки формы, содержащей одно или несколько полей.
    /// </summary>
    public class FormRowModel : ReactiveObject
    {
        private int _rowGroup = -1;

        /// <summary>
        /// Идентификатор группы для объединения полей в одну строку.
        /// </summary>
        public int RowGroup
        {
            get => _rowGroup;
            set => this.RaiseAndSetIfChanged(ref _rowGroup, value);
        }

        /// <summary>
        /// Коллекция полей, входящих в строку.
        /// </summary>
        public ObservableCollection<FormFieldModel> Fields { get; } = new();

    }
}
