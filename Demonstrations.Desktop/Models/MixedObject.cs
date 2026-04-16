using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Demonstrations.Desktop.Models
{
    public class MixedObject : ReactiveObject
    {
        private string _fullName = "Сергей Кузнецов";
        private int _age = 42;
        private DateTime _birthDate = new DateTime(1984, 7, 9);
        private string _email = "sergey@example.com";
        private string _phone = "+7 916 555 44 33";
        private string _position = "Ведущий разработчик";
        private double _salary = 150000;
        private bool _isRemote = false;
        private string _skills = "C#, Avalonia, SQL";
        private int _experience = 15;

        [Required]
        [DisplayName("Полное имя (из атрибута)")]
        public string FullName { get => _fullName; set => this.RaiseAndSetIfChanged(ref _fullName, value); }

        [Range(0, 100)]
        [DisplayName("Возраст (из атрибута)")]
        public int Age { get => _age; set => this.RaiseAndSetIfChanged(ref _age, value); }

        [DisplayName("Дата рождения")]
        public DateTime BirthDate { get => _birthDate; set => this.RaiseAndSetIfChanged(ref _birthDate, value); }

        [EmailAddress]
        [DisplayName("Email")]
        public string Email { get => _email; set => this.RaiseAndSetIfChanged(ref _email, value); }

        [Phone]
        [DisplayName("Телефон")]
        public string Phone { get => _phone; set => this.RaiseAndSetIfChanged(ref _phone, value); }

        // Без атрибутов, будет задано через конфиг
        public string Position { get => _position; set => this.RaiseAndSetIfChanged(ref _position, value); }

        // Без атрибутов
        public double Salary { get => _salary; set => this.RaiseAndSetIfChanged(ref _salary, value); }

        // Без атрибутов
        public bool IsRemote { get => _isRemote; set => this.RaiseAndSetIfChanged(ref _isRemote, value); }

        [StringLength(200)]
        [DisplayName("Навыки")]
        public string Skills { get => _skills; set => this.RaiseAndSetIfChanged(ref _skills, value); }

        [Range(1, 50)]
        [DisplayName("Стаж (лет)")]
        public int Experience { get => _experience; set => this.RaiseAndSetIfChanged(ref _experience, value); }
    }
}
