using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Common.Controls.Models
{
    public class CategoryModel : ReactiveObject
    {
        public CategoryModel(string name)
        {
            Name = name;
            Properties = new ObservableCollection<PropertyItemModel>();
        }

        public string Name { get; }
        public ObservableCollection<PropertyItemModel> Properties { get; }
    }
}
