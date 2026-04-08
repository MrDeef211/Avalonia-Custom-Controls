using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Common.Controls.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Subjects;
using System.Reflection;
using System.Windows.Input;

namespace Common.Controls;

public class DataFormControl : BaseEditorControl
{
    public ObservableCollection<FormSectionModel> Sections { get; } = new();

    #region Настраиваемые визуальные свойства

    public static readonly StyledProperty<double> LabelColumnWidthProperty =
        AvaloniaProperty.Register<DataFormControl, double>(nameof(LabelColumnWidth), 120.0);

    /// <summary>
    /// Ширина колонки имён свойств
    /// </summary>
    public double LabelColumnWidth
    {
        get => GetValue(LabelColumnWidthProperty);
        set => SetValue(LabelColumnWidthProperty, value);
    }

    public static readonly StyledProperty<Thickness> FieldMarginProperty =
        AvaloniaProperty.Register<DataFormControl, Thickness>(nameof(FieldMargin), new Thickness(4));

    /// <summary>
    /// Отступы полей
    /// </summary>
    public Thickness FieldMargin
    {
        get => GetValue(FieldMarginProperty);
        set => SetValue(FieldMarginProperty, value);
    }

    public static readonly StyledProperty<double> LabelFontSizeProperty =
        AvaloniaProperty.Register<DataFormControl, double>(nameof(LabelFontSize), 12.0);

    /// <summary>
    /// Размер шрифта колонки имён свойств
    /// </summary>
    public double LabelFontSize
    {
        get => GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }

    public static readonly StyledProperty<FontWeight> LabelFontWeightProperty =
        AvaloniaProperty.Register<DataFormControl, FontWeight>(nameof(LabelFontWeight), FontWeight.SemiBold);

    /// <summary>
    /// Тип шрифта колонки имён свойств
    /// </summary>
    public FontWeight LabelFontWeight
    {
        get => GetValue(LabelFontWeightProperty);
        set => SetValue(LabelFontWeightProperty, value);
    }

    public static readonly StyledProperty<IBrush?> LabelForegroundProperty =
        AvaloniaProperty.Register<DataFormControl, IBrush?>(nameof(LabelForeground), Brushes.Black);

    /// <summary>
    /// Цвет шрифта колонки имён свойств
    /// </summary>
    public IBrush? LabelForeground
    {
        get => GetValue(LabelForegroundProperty);
        set => SetValue(LabelForegroundProperty, value);
    }

    public static readonly StyledProperty<double> CategoryHeaderFontSizeProperty =
        AvaloniaProperty.Register<DataFormControl, double>(nameof(CategoryHeaderFontSize), 14.0);

    /// <summary>
    /// Размер шрифта заголовков
    /// </summary>
    public double CategoryHeaderFontSize
    {
        get => GetValue(CategoryHeaderFontSizeProperty);
        set => SetValue(CategoryHeaderFontSizeProperty, value);
    }

    public static readonly StyledProperty<FontWeight> CategoryHeaderFontWeightProperty =
        AvaloniaProperty.Register<DataFormControl, FontWeight>(nameof(CategoryHeaderFontWeight), FontWeight.Bold);

    /// <summary>
    /// Тип шрифта заголовков
    /// </summary>
    public FontWeight CategoryHeaderFontWeight
    {
        get => GetValue(CategoryHeaderFontWeightProperty);
        set => SetValue(CategoryHeaderFontWeightProperty, value);
    }

    public static readonly StyledProperty<ICommand?> SaveCommandProperty =
    AvaloniaProperty.Register<DataFormControl, ICommand?>(nameof(SaveCommand));

    /// <summary>
    /// Команда, выполняемая после успешного сохранения данных.
    /// </summary>
    /// <remarks>
    /// Параметр сохранённый обьект
    /// </remarks>
    public ICommand? SaveCommand
    {
        get => GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    #endregion

    protected override void GenerateEditors(object? target)
    {
        Sections.Clear();
        if (target == null) return;

        var properties = target.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => p.GetCustomAttribute<BrowsableAttribute>()?.Browsable ?? true);

        var grouped = properties
            .Select(p => new
            {
                Property = p,
                Category = p.GetCustomAttribute<CategoryAttribute>()?.Category ?? "Общие"
            })
            .GroupBy(g => g.Category)
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            var section = new FormSectionModel(group.Key);
            foreach (var item in group)
            {
                var fieldModel = new FormFieldModel(item.Property, target, IsReadOnly);
                fieldModel.WhenAnyValue(x => x.Value)
                    .Subscribe(Observer.Create<object?>(_ => OnFieldValueChanged(fieldModel)))
                    .DisposeWith(Subscriptions!);
                section.Fields.Add(fieldModel);
            }
            Sections.Add(section);
        }
    }

    private void OnFieldValueChanged(FormFieldModel fieldModel)
    {
        if (!string.IsNullOrEmpty(fieldModel.ValidationError))
            SetError(fieldModel.ValidationError);
        else
            SetError(string.Empty);

        UpdateHasChanges();
    }

    private void UpdateHasChanges()
    {
        bool hasChanges = Sections.SelectMany(s => s.Fields)
            .Any(f => !Equals(f.ConvertedValue, f.OriginalValue));
        SetHasChanges(hasChanges);
    }

    protected override void CommitChanges()
    {
        foreach (var field in Sections.SelectMany(s => s.Fields))
        {
            if (field.ValidationError != null && field.ValidationError != string.Empty)
                continue; 

            if (Equals(field.ConvertedValue, field.OriginalValue)) continue;
            field.PropertyInfo.SetValue(field.Target, field.ConvertedValue);
            field.UpdateOriginal();
        }
        SetHasChanges(false);
        if (SaveCommand?.CanExecute(SelectedObject) == true)
            SaveCommand.Execute(SelectedObject);
    }

    protected override void CancelChanges()
    {
        foreach (var field in Sections.SelectMany(s => s.Fields))
        {
            field.ResetToOriginal();
        }
        SetHasChanges(false);
    }
}