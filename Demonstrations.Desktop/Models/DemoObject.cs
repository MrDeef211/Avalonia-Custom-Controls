using ReactiveUI;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Demonstrations.Desktop.Models
{
    public class DemoObject : ReactiveObject
    {
        private string _name = "Новый объект";
        private int _age = 25;
        private bool _isActive = true;
        private double _progress = 0.75;
        private DateTime _creationDate = DateTime.Now;

        [DisplayName("Имя")]
        [Category("Ключи")]
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        [DisplayName("Возраст")]
        public int Age
        {
            get => _age;
            set => this.RaiseAndSetIfChanged(ref _age, value);
        }

        [DisplayName("Активен")]
        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
        }

        [DisplayName("Прогресс")]
        public double Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        [DisplayName("Дата создания")]
        [Category("Ключи")]
        public DateTime CreationDate
        {
            get => _creationDate;
            set => this.RaiseAndSetIfChanged(ref _creationDate, value);
        }

        public override string ToString() => Name;
    }
}