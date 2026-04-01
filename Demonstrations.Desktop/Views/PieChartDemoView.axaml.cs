using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Demonstrations.Desktop.ViewModels;
using System;
using System.Linq;

namespace Demonstrations.Desktop.Views;

public partial class PieChartDemoView : UserControl
{
    public PieChartDemoView()
    {
        InitializeComponent();
        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is PieChartDemoViewModel vm)
        {
            RegisterHandlers(vm);
        }
    }

    private void RegisterHandlers(PieChartDemoViewModel vm)
    {
        // Диалог выбора файла
        vm.ShowOpenFileDialog.RegisterHandler(async context =>
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите изображение",
                AllowMultiple = false,
                Filters = new System.Collections.Generic.List<FileDialogFilter>
                    {
                        new FileDialogFilter { Name = "Изображения", Extensions = { "png", "jpg", "jpeg", "bmp", "gif" } }
                    }
            };
            var owner = TopLevel.GetTopLevel(this) as Window;
            var result = await dialog.ShowAsync(owner);
            var path = result?.FirstOrDefault();
            context.SetOutput(path);
        });
    }
}
