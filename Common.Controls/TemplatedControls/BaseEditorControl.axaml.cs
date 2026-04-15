using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Subjects;

namespace Common.Controls;

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
    protected abstract void CommitChanges();

    /// <summary>
    /// Отменить изменения, восстановить исходное состояние.
    /// </summary>
    protected abstract void CancelChanges();

    /// <summary>
    /// Генерация полей / свойств на основе переданного объекта.
    /// </summary>
    protected abstract void GenerateEditors(object? target);

    #endregion

    #region Виртуальные методы

    protected virtual void OnActivated(CompositeDisposable disposables)
    {

    }

    protected virtual void OnSelectedObjectChanged(object? oldValue, object? newValue)
    {

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
}