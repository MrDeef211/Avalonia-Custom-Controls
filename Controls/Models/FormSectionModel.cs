using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Reactive;

namespace Controls.Models
{
    /// <summary>
    /// Модель секции (категории) формы.
    /// </summary>
    public class FormSectionModel : ReactiveObject
    {
        private bool _isExpanded = true;
        private bool _canCollapse;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FormSectionModel"/>.
        /// </summary>
        /// <param name="name">Название секции.</param>
        public FormSectionModel(string name)
        {
            Name = name;
            ToggleCommand = ReactiveCommand.Create(() =>
            {
                if (CanCollapse)
                    IsExpanded = !IsExpanded;
            });
        }

        /// <summary>
        /// Название секции.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Коллекция полей, входящих в секцию.
        /// </summary>
        public ObservableCollection<FormFieldModel> Fields { get; } = new();

        /// <summary>
        /// Коллекция строк, сгруппированных из полей.
        /// </summary>
        public ObservableCollection<FormRowModel> Rows { get; } = new();

        /// <summary>
        /// Флаг, указывающий, развёрнута ли секция.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        /// <summary>
        /// Флаг, указывающий, может ли пользователь сворачивать/разворачивать секцию.
        /// </summary>
        public bool CanCollapse
        {
            get => _canCollapse;
            set => this.RaiseAndSetIfChanged(ref _canCollapse, value);
        }

        /// <summary>
        /// Команда переключения состояния свёрнутости.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ToggleCommand { get; }
    }
}
