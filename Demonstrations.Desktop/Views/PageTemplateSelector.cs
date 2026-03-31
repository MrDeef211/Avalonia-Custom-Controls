using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demonstrations.Desktop.Views
{
    public class PageTemplateSelector : IDataTemplate
    {
        public Dictionary<Type, IDataTemplate> Templates { get; } = new();

        public Control Build(object param)
        {
            var type = param.GetType();
            if (Templates.TryGetValue(type, out var template))
                return template.Build(param);
            return new TextBlock { Text = "No template for " + type.Name };
        }

        public bool Match(object data) => true;
    }
}
