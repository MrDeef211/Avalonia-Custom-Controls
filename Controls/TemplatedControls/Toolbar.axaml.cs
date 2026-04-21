using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using System.Collections.Generic;

namespace Controls;

public class Toolbar : Avalonia.Controls.ItemsControl
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Toolbar, Orientation>(nameof(Orientation), Orientation.Horizontal);

    public static readonly StyledProperty<bool> WrapProperty =
        AvaloniaProperty.Register<Toolbar, bool>(nameof(Wrap), false);

    /// <summary>
    /// Ориентация Toolbar в пространстве (горизонтально или вертикально)
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Переносить компоненты на новую строку при нехватке места.
    /// </summary>
    public bool Wrap
    {
        get => GetValue(WrapProperty);
        set => SetValue(WrapProperty, value);
    }

    static Toolbar()
    {
        OrientationProperty.Changed.AddClassHandler<Toolbar>((x, _) => x.UpdateClasses());
        WrapProperty.Changed.AddClassHandler<Toolbar>((x, _) => x.UpdateClasses());
    }

    public Toolbar()
    {
        UpdateClasses();
    }

    private void UpdateClasses()
    {
        Classes.Remove("vertical");
        Classes.Remove("horizontal");
        Classes.Remove("wrap");

        Classes.Add(Orientation == Orientation.Vertical ? "vertical" : "horizontal");
        if (Wrap) Classes.Add("wrap");
    }
}