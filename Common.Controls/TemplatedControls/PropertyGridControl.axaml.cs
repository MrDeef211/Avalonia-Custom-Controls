using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Common.Controls.Models;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Subjects;
using System.Reflection;

namespace Common.Controls;

public class PropertyGridControl : BaseEditorControl
{
    public ObservableCollection<CategoryModel> Categories { get; } = new();

    #region Настраиваемые визуальные свойства

    public static readonly StyledProperty<GridLength> PropertyNameColumnWidthProperty =
        AvaloniaProperty.Register<PropertyGridControl, GridLength>(nameof(PropertyNameColumnWidth), GridLength.Auto);

    /// <summary>
    /// Ширина колонки имён свойств (по умолчанию Auto, но можно задать фиксированную)
    /// </summary>
    public GridLength PropertyNameColumnWidth
    {
        get => GetValue(PropertyNameColumnWidthProperty);
        set => SetValue(PropertyNameColumnWidthProperty, value);
    }

    
    public static readonly StyledProperty<Thickness> EditorMarginProperty =
        AvaloniaProperty.Register<PropertyGridControl, Thickness>(nameof(EditorMargin), new Thickness(4));


    /// <summary>
    /// Отступы внутри редактора
    /// </summary>
    public Thickness EditorMargin
    {
        get => GetValue(EditorMarginProperty);
        set => SetValue(EditorMarginProperty, value);
    }

    public static readonly StyledProperty<double> PropertyNameFontSizeProperty =
        AvaloniaProperty.Register<PropertyGridControl, double>(nameof(PropertyNameFontSize), 12.0);

    /// <summary>
    /// Шрифт для имени свойства
    /// </summary>
    public double PropertyNameFontSize
    {
        get => GetValue(PropertyNameFontSizeProperty);
        set => SetValue(PropertyNameFontSizeProperty, value);
    }

    public static readonly StyledProperty<FontWeight> PropertyNameFontWeightProperty =
        AvaloniaProperty.Register<PropertyGridControl, FontWeight>(nameof(PropertyNameFontWeight), FontWeight.SemiBold);

    /// <summary>
    /// Тип шрифта для имени свойства
    /// </summary>
    public FontWeight PropertyNameFontWeight
    {
        get => GetValue(PropertyNameFontWeightProperty);
        set => SetValue(PropertyNameFontWeightProperty, value);
    }

    public static readonly StyledProperty<IBrush?> PropertyNameForegroundProperty =
        AvaloniaProperty.Register<PropertyGridControl, IBrush?>(nameof(PropertyNameForeground), Brushes.Black);

    public IBrush? PropertyNameForeground
    {
        get => GetValue(PropertyNameForegroundProperty);
        set => SetValue(PropertyNameForegroundProperty, value);
    }
    
    public static readonly StyledProperty<IBrush?> ErrorIconForegroundProperty =
        AvaloniaProperty.Register<PropertyGridControl, IBrush?>(nameof(ErrorIconForeground), Brushes.Red);

    /// <summary>
    /// Цвет иконки ошибки
    /// </summary>
    public IBrush? ErrorIconForeground
    {
        get => GetValue(ErrorIconForegroundProperty);
        set => SetValue(ErrorIconForegroundProperty, value);
    }
    
    public static readonly StyledProperty<double> CategoryHeaderFontSizeProperty =
        AvaloniaProperty.Register<PropertyGridControl, double>(nameof(CategoryHeaderFontSize), 14.0);

    /// <summary>
    /// Шрифт для заголовков категорий
    /// </summary>
    public double CategoryHeaderFontSize
    {
        get => GetValue(CategoryHeaderFontSizeProperty);
        set => SetValue(CategoryHeaderFontSizeProperty, value);
    }

    public static readonly StyledProperty<FontWeight> CategoryHeaderFontWeightProperty =
        AvaloniaProperty.Register<PropertyGridControl, FontWeight>(nameof(CategoryHeaderFontWeight), FontWeight.Bold);

    /// <summary>
    /// Тип шрифта для заголовков категорий
    /// </summary>
    public FontWeight CategoryHeaderFontWeight
    {
        get => GetValue(CategoryHeaderFontWeightProperty);
        set => SetValue(CategoryHeaderFontWeightProperty, value);
    }

    #endregion

    protected override void GenerateEditors(object? target)
    {
        Categories.Clear();
        if (target == null) return;

        var properties = GetPublicProperties(target);

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
            var categoryModel = new CategoryModel(group.Key);
            foreach (var item in group)
            {
                bool readOnly = IsPropertyReadOnly(item.Property);
                var propVm = new PropertyItemModel(item.Property, target, readOnly);
                propVm.WhenAnyValue(x => x.Value)
                    .Subscribe(Observer.Create<object?>(_ => OnPropertyValueChanged(propVm)))
                    .DisposeWith(Subscriptions!);

                categoryModel.Properties.Add(propVm);
            }
            Categories.Add(categoryModel);
        }
    }

    private void OnPropertyValueChanged(PropertyItemModel propertyModel)
    {
        propertyModel.ValidationError = string.Empty;
        SetError(string.Empty);

        var currentValue = propertyModel.PropertyInfo.GetValue(propertyModel.Target);

        if (propertyModel.Value == null)
        {
            var propertyType = propertyModel.PropertyInfo.PropertyType;
            if (propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null)
            {
                propertyModel.ValidationError = "Значение не может быть null";
                SetError(propertyModel.ValidationError);
                return;
            }

            if (currentValue == null)
                return;

            propertyModel.PropertyInfo.SetValue(propertyModel.Target, null);
            SetHasChanges(true);
            return;
        }

        var targetType = Nullable.GetUnderlyingType(propertyModel.PropertyInfo.PropertyType) ?? propertyModel.PropertyInfo.PropertyType;
        object? convertedValue;
        try
        {
            convertedValue = Convert.ChangeType(propertyModel.Value, targetType);
        }
        catch (Exception ex)
        {
            propertyModel.ValidationError = $"Ошибка: {ex.Message}";
            SetError(propertyModel.ValidationError);
            return;
        }

        if (Equals(currentValue, convertedValue))
            return;

        propertyModel.PropertyInfo.SetValue(propertyModel.Target, convertedValue);
        SetHasChanges(true);
    }

    protected override void CommitChanges()
    {
        SetHasChanges(false);
    }

    protected override void CancelChanges()
    {
        GenerateEditors(SelectedObject);
        SetHasChanges(false);
    }
}