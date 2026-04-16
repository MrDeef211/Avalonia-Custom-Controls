using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;

namespace Controls;

public class ToolbarGroup : ItemsControl
{
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<ToolbarGroup, object?>(nameof(Header));

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    static ToolbarGroup()
    {
        ItemsPanelProperty.OverrideDefaultValue<ToolbarGroup>(new ItemsPanelTemplate
        {
            Content = new FuncTemplate<Panel?>(() => new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4
            })
        });

        HeaderProperty.Changed.AddClassHandler<ToolbarGroup>((x, _) => x.UpdateHeader());
    }

    private void UpdateHeader() => InvalidateMeasure();
}