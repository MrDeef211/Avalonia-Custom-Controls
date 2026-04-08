using Avalonia.Controls;
using Demonstrations.Desktop.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

namespace Demonstrations.Desktop.Views
{
    public partial class PieChartDemoView : ReactiveUserControl<PieChartDemoViewModel>
    {
        public PieChartDemoView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                // Регистрация диалога выбора файла
                ViewModel!.ShowOpenFileDialog.RegisterHandler(async context =>
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
                }).DisposeWith(disposables);
            });
        }
    }
}