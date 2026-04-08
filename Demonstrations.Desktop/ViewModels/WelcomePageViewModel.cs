using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Demonstrations.Desktop.ViewModels
{
    public class WelcomePageViewModel : PageViewModelBase
    {
        public WelcomePageViewModel()
        {
            Title = "Главная";
        }

        // Коллекция описаний контролов
        public ObservableCollection<ControlDescription> Controls { get; } = new()
        {
            new ControlDescription
            {
                Name = "ChartControl",
                Description = "Отображение графиков и диаграмм. Поддерживает различные стили (линии, столбцы, точки), заливку, сетку, подписи осей."
            },
            new ControlDescription
            {
                Name = "PieChartControl",
                Description = "Круговая диаграмма для отображения долей. Позволяет настраивать цвета."
            },
            new ControlDescription
            {
                Name = "RangeSliderControl",
                Description = "Двойной ползунок для выбора диапазона значений. Поддерживает минимальное и максимальное ограничения, шаг."
            },
            new ControlDescription
            {
                Name = "PropertyGridControl",
                Description = "Редактор свойств объектов. Автоматически отображает все публичные свойства, группирует по категориям, поддерживает редактирование на лету."
            },
            new ControlDescription
            {
                Name = "DataFormControl",
                Description = "Форма для редактирования данных. Поля группируются по категориям, поддерживает буферизацию изменений, кнопки «Сохранить»/«Отмена», настраиваемую сетку и выборочное отображение свойств."
            },
            new ControlDescription
            {
                Name = "ZoomControl",
                Description = "Панель для подробного просмотра содержимого, потдерживает масштабирование, перетаскивание, историю перемещений (Undo/Redo для положений камеры), горячие клавиши."
            }
        };
    }

    public class ControlDescription
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
