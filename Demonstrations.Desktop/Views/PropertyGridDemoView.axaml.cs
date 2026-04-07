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
    }
}