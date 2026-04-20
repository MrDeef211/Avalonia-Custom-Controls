using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Controls.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Controls.Selectors
{
    public class RowTemplateSelector : IDataTemplate
    {
        public IDataTemplate? SingleFieldTemplate { get; set; }
        public IDataTemplate? MultiFieldTemplate { get; set; }

        public Control? Build(object? param)
        {
            if (param is FormRowModel row)
            {
                return row.Fields.Count == 1
                    ? SingleFieldTemplate?.Build(param)
                    : MultiFieldTemplate?.Build(param);
            }
            return null;
        }

        public bool Match(object? data) => data is FormRowModel;
    }
}
