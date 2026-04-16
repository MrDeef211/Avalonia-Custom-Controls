using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Subjects;
using System.Reflection;

namespace Controls;

/// <summary>
/// Абстрактный базовый контрол для контролов вроде PropertyGrid и DataForm.
/// </summary>
public abstract class BaseEditorControl : TemplatedControl
{
    private CompositeDisposable? _subscriptions;
    protected CompositeDisposable? Subscriptions => _subscriptions;

    #region Styled Properties

    public static readonly StyledProperty<object?> SelectedObjectProperty =
    AvaloniaProperty.Register<BaseEditorControl, object?>(nameof(SelectedObject));

    public object? SelectedObject
    {
        get => GetValue(SelectedObjectProperty);
        set => SetValue(SelectedObjectProperty, value);
    }

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<BaseEditorControl, bool>(nameof(IsReadOnly));

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly StyledProperty<bool> HasChangesProperty =
    AvaloniaProperty.Register<BaseEditorControl, bool>(nameof(HasChanges), defaultBindingMode: BindingMode.TwoWay);

    public bool HasChanges
    {
        get => GetValue(HasChangesProperty);
        protected set => SetValue(HasChangesProperty, value);
    }

    public static readonly StyledProperty<string> ErrorProperty =
        AvaloniaProperty.Register<BaseEditorControl, string>(nameof(Error), defaultBindingMode: BindingMode.TwoWay);

    public string Error
    {
        get => GetValue(ErrorProperty);
        protected set => SetValue(ErrorProperty, value);
    }

    #endregion

    #region Direct Properties

    #endregion

    #region Абстрактные методы

    /// <summary>
    /// Применить изменения к редактируемому объекту.
    /// </summary>
    protected internal abstract void CommitChanges();

    /// <summary>
    /// Отменить изменения, восстановить исходное состояние.
    /// </summary>
    protected internal abstract void CancelChanges();

    /// <summary>
    /// Генерация полей / свойств на основе переданного объекта.
    /// </summary>
    protected internal abstract void GenerateEditors(object? target);

    #endregion

    #region Виртуальные методы

    protected virtual void OnActivated(CompositeDisposable disposables)
    {

    }

    protected virtual void OnSelectedObjectChanged(object? oldValue, object? newValue)
    {

    }

    protected virtual bool IsReactiveInternalProperty(PropertyInfo prop)
    {
        if (prop.DeclaringType == typeof(ReactiveObject) ||
            prop.DeclaringType == typeof(IReactiveObject))
            return true;

        string name = prop.Name;
        return name == "Changing" || name == "Changed" || name == "ThrownExceptions";
    }

    #endregion

    #region Управление подписками через визуальное дерево

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _subscriptions = new CompositeDisposable();

        this.GetObservable(SelectedObjectProperty)
            .Subscribe(Observer.Create<object?>(obj =>
            {
                GenerateEditors(obj);
                HasChanges = false;
                Error = string.Empty;
            }))
            .DisposeWith(_subscriptions);

        OnActivated(_subscriptions);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _subscriptions?.Dispose();
        _subscriptions = null;
        base.OnDetachedFromVisualTree(e);
    }

    #endregion

    #region Переопределение методов Avalonia

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedObjectProperty)
        {
            OnSelectedObjectChanged(change.OldValue, change.NewValue);
        }
    }

    /// <summary>
    /// Защищённый метод для установки флага изменений из наследников.
    /// </summary>
    protected void SetHasChanges(bool hasChanges) => HasChanges = hasChanges;

    /// <summary>
    /// Защищённый метод для установки ошибки валидации.
    /// </summary>
    protected void SetError(string error) => Error = error;

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Возвращает отфильтрованный список публичных свойств экземпляра,
    /// исключая служебные свойства ReactiveUI.
    /// </summary>
    protected IEnumerable<PropertyInfo> GetPublicProperties(object target)
    {
        if (target == null) return Enumerable.Empty<PropertyInfo>();

        return target.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .Where(p => !IsReactiveInternalProperty(p));
    }

    /// <summary>
    /// Определяет, должно ли свойство быть только для чтения в UI.
    /// Учитывает глобальный IsReadOnly, наличие публичного сеттера.
    /// </summary>
    protected bool IsPropertyReadOnly(PropertyInfo property)
    {
        if (IsReadOnly) return true;
        if (!property.CanWrite) return true;
        return property.SetMethod?.IsPublic != true;
    }

    /// <summary>
    /// Нормализует имя категории (удаляет пробелы, приводит к единому регистру).
    /// </summary>
    protected static string NormalizeCategoryName(string? category)
    {
        return string.IsNullOrWhiteSpace(category) ? "Общие" : category.Trim();
    }

    protected void EnsureSubscriptions()
    {
        _subscriptions ??= new CompositeDisposable();
    }

    #endregion
}