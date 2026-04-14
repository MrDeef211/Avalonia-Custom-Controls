using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Reactive;

namespace Common.Controls.Models
{
    public class FormSectionModel : ReactiveObject
    {
        private string _name;
        private bool _isExpanded = true;
        private bool _canCollapse = false;

        public FormSectionModel(string name)
        {
            Name = name;
            ToggleCommand = ReactiveCommand.Create(() =>
            {
                if (CanCollapse)
                    IsExpanded = !IsExpanded;
            });
        }

        public string Name { get; }
        public ObservableCollection<FormFieldModel> Fields { get; } = new();

        public ObservableCollection<FormRowModel> Rows { get; } = new();

        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        public bool CanCollapse
        {
            get => _canCollapse;
            set => this.RaiseAndSetIfChanged(ref _canCollapse, value);
        }

        public ReactiveCommand<Unit, Unit> ToggleCommand { get; }
    }
}
