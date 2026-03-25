using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;
using System.Windows.Input;

namespace Common.Controls.TemplatedControls;

[PseudoClasses(":pressed", ":canPressed")]
public class AnimatedButtonBasic : TemplatedControl
{
    public static readonly StyledProperty<string> ContentProperty =
    AvaloniaProperty.Register<AnimatedButtonBasic, string>(nameof(Content), "Button");


    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<AnimatedButtonBasic, ICommand?>(nameof(Command));

    public string Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            PseudoClasses.Set(":pressed", true);
            Command?.Execute(null);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        PseudoClasses.Set(":pressed", false);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        PseudoClasses.Set(":pressed", false);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CommandProperty)
        {
            var oldCommand = change.OldValue as ICommand;
            var newCommand = change.NewValue as ICommand;

            if (oldCommand != null) oldCommand.CanExecuteChanged -= OnCommandCanExecuteChanged;
            if (newCommand != null) newCommand.CanExecuteChanged += OnCommandCanExecuteChanged;

            UpdateCanPressed();
        }
    }

    private void OnCommandCanExecuteChanged(object? sender, EventArgs e) => UpdateCanPressed();

    private void UpdateCanPressed()
    {
        PseudoClasses.Set(":canPressed", Command?.CanExecute(null) ?? true);
    }
}