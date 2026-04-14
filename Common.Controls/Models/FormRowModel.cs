using Avalonia.Layout;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Common.Controls.Models
{
    /// <summary>
    /// Модель строки формы, содержащей одно или несколько полей.
    /// </summary>
    public class FormRowModel : ReactiveObject
    {
        private int _rowGroup = -1;
        private Orientation _orientation = Orientation.Vertical;

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

        /// <summary>
        /// Ориентация расположения полей в строке.
        /// По умолчанию Vertical (одно поле), Horizontal если полей несколько.
        /// </summary>
        public Orientation Orientation
        {
            get => _orientation;
            set => this.RaiseAndSetIfChanged(ref _orientation, value);
        }
    }
}
