using Avalonia;
using Avalonia.Controls.Primitives;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

namespace Common.Controls;

/// <summary>
/// Абстрактный базовый контрол для контролов вроде PropertyGrid и DataForm.
/// </summary>
public abstract class BaseEditorControl : TemplatedControl
{
    private CompositeDisposable? _subscriptions;
    protected CompositeDisposable? Subscriptions => _subscriptions;

    private bool _hasChanges;
    private string _error = string.Empty;


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

    #endregion

    #region Direct Properties

    public static readonly DirectProperty<BaseEditorControl, bool> HasChangesProperty =
            AvaloniaProperty.RegisterDirect<BaseEditorControl, bool>(nameof(HasChanges), o => o.HasChanges);

    public bool HasChanges
    {
        get => _hasChanges;
        protected set => SetAndRaise(HasChangesProperty, ref _hasChanges, value);
    }

    public static readonly DirectProperty<BaseEditorControl, string> ErrorProperty =
        AvaloniaProperty.RegisterDirect<BaseEditorControl, string>(nameof(Error), o => o.Error);

    public string Error
    {
        get => _error;
        protected set => SetAndRaise(ErrorProperty, ref _error, value);
    }

    #endregion

    #region Комманды

    private ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>? _commitChangesCommand;
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> CommitChangesCommand =>
        _commitChangesCommand ??= ReactiveCommand.Create(CommitChanges);

    private ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>? _cancelChangesCommand;
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> CancelChangesCommand =>
        _cancelChangesCommand ??= ReactiveCommand.Create(CancelChanges);

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