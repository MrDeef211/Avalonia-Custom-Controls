using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demonstrations.Desktop.Models
{
    public class ConfigOnlyObject : ReactiveObject
    {
        private string _fullName = "Анна Смирнова";
        private int _age = 28;
        private DateTime _birthDate = new DateTime(1998, 3, 22);
        private string _email = "anna@example.com";
        private string _phone = "+7 999 123 45 67";
        private string _department = "Маркетинг";
        private double _salary = 85000;
        private bool _isFullTime = true;
        private DateTime _hireDate = DateTime.Now.AddYears(-2);
        private string _notes = "Без атрибутов, только конфиг";

        public string FullName { get => _fullName; set => this.RaiseAndSetIfChanged(ref _fullName, value); }
        public int Age { get => _age; set => this.RaiseAndSetIfChanged(ref _age, value); }
        public DateTime BirthDate { get => _birthDate; set => this.RaiseAndSetIfChanged(ref _birthDate, value); }
        public string Email { get => _email; set => this.RaiseAndSetIfChanged(ref _email, value); }
        public string Phone { get => _phone; set => this.RaiseAndSetIfChanged(ref _phone, value); }
        public string Department { get => _department; set => this.RaiseAndSetIfChanged(ref _department, value); }
        public double Salary { get => _salary; set => this.RaiseAndSetIfChanged(ref _salary, value); }
        public bool IsFullTime { get => _isFullTime; set => this.RaiseAndSetIfChanged(ref _isFullTime, value); }
        public DateTime HireDate { get => _hireDate; set => this.RaiseAndSetIfChanged(ref _hireDate, value); }
        public string Notes { get => _notes; set => this.RaiseAndSetIfChanged(ref _notes, value); }
    }
}
