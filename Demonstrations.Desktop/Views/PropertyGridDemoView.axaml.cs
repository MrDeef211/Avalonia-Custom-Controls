using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Common.Controls;
using Demonstrations.Desktop.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System.Reactive;
using System.Reactive.Disposables.Fluent;

namespace Demonstrations.Desktop.Views;

public partial class PropertyGridDemoView : ReactiveUserControl<PropertyGridDemoViewModel>
{
    public PropertyGridDemoView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            PropertyGrid.PropertyChanged += (sender, e) =>
            {
                if (e.Property == PropertyGridControl.HasChangesProperty)
                {
                    if (ViewModel != null)
                        ViewModel.HasChanges = (bool)e.NewValue;
                }
                else if (e.Property == PropertyGridControl.ErrorProperty)
                {
                    if (ViewModel != null)
                        ViewModel.Error = (string?)e.NewValue ?? string.Empty;
                }
            };
        });
    }
}