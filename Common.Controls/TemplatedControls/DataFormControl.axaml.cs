using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Common.Controls.Models;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Subjects;
using System.Reflection;
using System.Windows.Input;

namespace Common.Controls;

public class DataFormControl : BaseEditorControl, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private readonly CompositeDisposable _fieldSubscriptions = new();

    public DataFormControl()
    {

        var canExecute = this.WhenAnyValue(x => x.HasChanges);

        CommitChangesCommand = ReactiveCommand.Create(() => CommitChanges(), canExecute);
        CancelChangesCommand = ReactiveCommand.Create(() => CancelChanges(), canExecute);
    }

    public ObservableCollection<FormSectionModel> Sections { get; } = new();

    #region Настраиваемые визуальные свойства

    public static readonly StyledProperty<DataFormConfig?> FormConfigProperty =
        AvaloniaProperty.Register<DataFormControl, DataFormConfig?>(nameof(FormConfig));

    /// <summary>
    /// Конфигурация формы
    /// </summary>
    public DataFormConfig? FormConfig
    {
        get => GetValue(FormConfigProperty);
        set => SetValue(FormConfigProperty, value);
    }

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
    /// Параметр сохранённый объект
    /// </remarks>
    public ICommand? SaveCommand
    {
        get => GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> CancelCommandProperty =
        AvaloniaProperty.Register<DataFormControl, ICommand?>(nameof(CancelCommand));

    /// <summary>
    /// Команда, выполняемая после успешной отмены изменений.
    /// </summary>
    /// <remarks>
    /// Параметр обьект
    /// </remarks>
    public ICommand? CancelCommand
    {
        get => GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    public static readonly StyledProperty<ButtonPanelPlacement> ButtonPanelPlacementProperty =
        AvaloniaProperty.Register<DataFormControl, ButtonPanelPlacement>(nameof(ButtonPanelPlacement), ButtonPanelPlacement.None);

    /// <summary>
    /// Расположение кнопок "сохранить" и "отмена"
    /// </summary>
    public ButtonPanelPlacement ButtonPanelPlacement
    {
        get => GetValue(ButtonPanelPlacementProperty);
        set => SetValue(ButtonPanelPlacementProperty, value);
    }

    #endregion


    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> CommitChangesCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> CancelChangesCommand { get; }

    protected override void OnSelectedObjectChanged(object? oldValue, object? newValue)
    {
        base.OnSelectedObjectChanged(oldValue, newValue);
        GenerateEditors(newValue);
    }

    protected override void GenerateEditors(object? target)
    {
        _fieldSubscriptions.Clear();
        foreach (var section in Sections)
        {
            foreach (var field in section.Fields)
            {
                field.Dispose();
            }
        }
        Sections.Clear();

        if (target == null) return;

        var properties = target.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .ToList();

        var configuredProperties = new List<(PropertyInfo Property, DataFormFieldConfig? Config)>();
        foreach (var prop in properties)
        {
            bool browsable = true;
            DataFormFieldConfig? cfg = null;
            if (FormConfig?.Fields.TryGetValue(prop.Name, out cfg) == true)
                browsable = cfg.IsBrowsable;
            else
                browsable = prop.GetCustomAttribute<BrowsableAttribute>()?.Browsable ?? true;

            if (browsable)
                configuredProperties.Add((prop, cfg));
        }

        var itemsWithCategory = new List<(PropertyInfo Property, DataFormFieldConfig? Config, string Category, int CategoryOrder, int Order)>();
        foreach (var item in configuredProperties)
        {
            var rawCategory = item.Config?.Category ?? item.Property.GetCustomAttribute<CategoryAttribute>()?.Category ?? "Общие";
            var normalizedCategory = NormalizeCategoryName(rawCategory);
            int categoryOrder = 0;
            if (FormConfig?.CategoryOrders != null && FormConfig.CategoryOrders.TryGetValue(normalizedCategory, out var orderFromDict))
                categoryOrder = orderFromDict;
            else if (item.Config?.CategoryOrder.HasValue == true)
                categoryOrder = item.Config.CategoryOrder.Value;

            itemsWithCategory.Add((item.Property, item.Config, rawCategory, categoryOrder, item.Config?.Order ?? 0));
        }

        var grouped = itemsWithCategory
            .GroupBy(g => g.Category)
            .OrderBy(g => g.First().CategoryOrder)
            .ThenBy(g => g.Key);

        foreach (var group in grouped)
        {
            var section = new FormSectionModel(group.Key);
            var firstConfig = group.First().Config;
            if (FormConfig?.CategoryExpanded.TryGetValue(group.Key, out var expanded) == true)
                section.IsExpanded = expanded;
            else
                section.IsExpanded = firstConfig?.IsCategoryExpanded ?? true;
            if (FormConfig?.CategoryCollapsible.TryGetValue(group.Key, out var canCollapse) == true)
                section.CanCollapse = canCollapse;

            var sortedFields = group.OrderBy(g => g.Order).ToList();
            foreach (var item in sortedFields)
            {
                var fieldModel = new FormFieldModel(item.Property, target, IsReadOnly, item.Config);
                section.Fields.Add(fieldModel);
            }
            Sections.Add(section);
            BuildRows(section);
        }

        _fieldSubscriptions.Clear();
        foreach (var field in Sections.SelectMany(s => s.Fields))
        {
            var sub = field.WhenAnyValue(
                f => f.IsTouched,
                f => f.ConvertedValue,
                f => f.OriginalValue,
                (touched, converted, original) => touched && !Equals(converted, original))
                .Subscribe(_ => UpdateHasChanges())
                .DisposeWith(_fieldSubscriptions);
        }
        UpdateHasChanges();
    }

    private void BuildRows(FormSectionModel section)
    {
        section.Rows.Clear();
        var groups = section.Fields.GroupBy(f => f.RowGroup);
        foreach (var group in groups)
        {
            if (group.Key == -1)
            {
                foreach (var field in group)
                {
                    var row = new FormRowModel { RowGroup = -1 };
                    row.Fields.Add(field);
                    section.Rows.Add(row);
                }
            }
            else
            {
                var row = new FormRowModel { RowGroup = group.Key };
                foreach (var field in group)
                    row.Fields.Add(field);
                section.Rows.Add(row);
            }
        }
    }

    private void UpdateHasChanges()
    {
        bool hasChanges = Sections.SelectMany(s => s.Fields)
            .Any(f => f.IsTouched && !Equals(f.ConvertedValue, f.OriginalValue));
        SetHasChanges(hasChanges);
    }

    protected override void CommitChanges()
    {
        foreach (var field in Sections.SelectMany(s => s.Fields))
        {
            if (!string.IsNullOrEmpty(field.ValidationError))
                continue;
            if (Equals(field.ConvertedValue, field.OriginalValue)) continue;
            field.PropertyInfo.SetValue(field.Target, field.ConvertedValue);
            field.UpdateOriginal();
        }
        SaveCommand?.Execute(SelectedObject);
    }

    protected override void CancelChanges()
    {
        foreach (var field in Sections.SelectMany(s => s.Fields))
        {
            field.ResetToOriginal();
        }
        CancelCommand?.Execute(SelectedObject);
    }

    private static string NormalizeCategoryName(string category)
    {
        return category?.Trim() ?? "Общие";
    }

    public void Dispose()
    {
        _fieldSubscriptions.Dispose();
        _disposables.Dispose();
        foreach (var section in Sections)
        {
            foreach (var field in section.Fields)
            {
                field.Dispose();
            }
        }
    }
}

public enum ButtonPanelPlacement
{
    /// <summary>Кнопки не отображаются</summary>
    None,
    /// <summary>Кнопки сверху, вне прокручиваемой области</summary>
    Top,
    /// <summary>Кнопки снизу, вне прокручиваемой области</summary>
    Bottom,
    /// <summary>Кнопки сверху, внутри прокручиваемой области (перед полями)</summary>
    InsideTop,
    /// <summary>Кнопки снизу, внутри прокручиваемой области (после полей)</summary>
    InsideBottom
}