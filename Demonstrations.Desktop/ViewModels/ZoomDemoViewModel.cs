using Avalonia.Media.Imaging;
using Controls.Models;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Demonstrations.Desktop.ViewModels
{
    public class ZoomDemoViewModel : PageViewModelBase
    {
        private ZoomState _zoomState = new();
        private Bitmap? _currentImage;

        public ZoomDemoViewModel()
        {
            Title = "ZoomControl - Масштабирование и панорамирование";

            // Интеракция для выбора файла
            ShowOpenFileDialog = new Interaction<Unit, string?>();

            // Команда выбора изображения
            PickImageCommand = ReactiveCommand.CreateFromTask(PickImageAsync);
        }

        public ZoomState ZoomState
        {
            get => _zoomState;
            set => this.RaiseAndSetIfChanged(ref _zoomState, value);
        }

        public Bitmap? CurrentImage
        {
            get => _currentImage;
            set => this.RaiseAndSetIfChanged(ref _currentImage, value);
        }

        public Interaction<Unit, string?> ShowOpenFileDialog { get; }

        public ReactiveCommand<Unit, Unit> PickImageCommand { get; }

        private async Task PickImageAsync()
        {
            var path = await ShowOpenFileDialog.Handle(Unit.Default);
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    var bitmap = await Task.Run(() => new Bitmap(path));
                    CurrentImage = bitmap;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки: {ex.Message}");
                }
            }
        }
    }
}