using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Demonstrations.Desktop.ViewModels;
using System;

namespace Demonstrations.Desktop.Views;

public partial class CombinedFirstDemoView : UserControl
{
    public CombinedFirstDemoView()
    {
        InitializeComponent();
    }

    public void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is CombinedFirstDemoViewModel vm)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                double center = (vm.UpperX + vm.LowerX) / 2;
                double dx = (vm.UpperX - vm.LowerX) / 2;
                double delta = Math.Truncate(dx * 0.2) > 0 ? Math.Truncate(dx * 0.2) : 1;

                if (e.Delta.Y > 0)
                {
                    vm.UpperX -= (vm.UpperX - delta) > center ? delta : 0;
                    vm.LowerX += (vm.LowerX + delta) < center ? delta : 0;
                }
                else
                {
                    vm.UpperX = (vm.UpperX + delta) < 500 ? vm.UpperX + delta : 500;
                    vm.LowerX = (vm.LowerX - delta) > -500 ? vm.LowerX - delta : -500;
                }

                e.Handled = true;
            }
        }
    }
}