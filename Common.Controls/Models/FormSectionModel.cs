using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Common.Controls.Models
{
    public class FormSectionModel : ReactiveObject
    {
        public FormSectionModel(string name)
        {
            Name = name;
            Fields = new ObservableCollection<FormFieldModel>();
        }

        public string Name { get; }
        public ObservableCollection<FormFieldModel> Fields { get; }
    }
}
