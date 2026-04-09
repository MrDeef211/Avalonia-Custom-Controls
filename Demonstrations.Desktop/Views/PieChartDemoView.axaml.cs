using Avalonia.Controls;
using Avalonia.Platform.Storage;
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
                ViewModel!.ShowOpenFileDialog.RegisterHandler(async context =>
                {

                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel == null) return;

                    var options = new FilePickerOpenOptions
                    {
                        Title = "Выберите изображение",
                        AllowMultiple = false,
                        FileTypeFilter = new[]
                        {
                new FilePickerFileType("Изображения")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" },
                    MimeTypes = new[] { "image/*" }
                }
            }
                    };

                    var result = await topLevel.StorageProvider.OpenFilePickerAsync(options);

                    var selectedFile = result.FirstOrDefault();
                    var path = selectedFile?.Path.LocalPath;

                    context.SetOutput(path);
                }).DisposeWith(disposables);
            });
        }
    }
}