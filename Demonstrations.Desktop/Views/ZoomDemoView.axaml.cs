using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Demonstrations.Desktop.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

namespace Demonstrations.Desktop.Views
{
    public partial class ZoomDemoView : ReactiveUserControl<ZoomDemoViewModel>
    {
        public ZoomDemoView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                ViewModel!.ShowOpenFileDialog.RegisterHandler(async interaction =>
                {
                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel == null)
                    {
                        interaction.SetOutput(null);
                        return;
                    }

                    var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "Выберите изображение",
                        AllowMultiple = false,
                        FileTypeFilter = new[]
                        {
                            new FilePickerFileType("Изображения")
                            {
                                Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" }
                            }
                        }
                    });

                    var path = files.FirstOrDefault()?.Path.LocalPath;
                    interaction.SetOutput(path);
                }).DisposeWith(disposables);
            });
        }
    }
}