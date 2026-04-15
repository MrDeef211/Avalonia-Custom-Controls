using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Demonstrations.Desktop.Models
{
    public enum EmployeeRole
    {
        [Description("Менеджер")]
        Manager,
        [Description("Разработчик")]
        Developer,
        [Description("Дизайнер")]
        Designer,
        [Description("Тестировщик")]
        Tester
    }

    public enum WorkShift
    {
        [Description("Утро")]
        Morning,
        [Description("День")]
        Afternoon,
        [Description("Ночь")]
        Night,
        [Description("День")]
        Day
    }

    public enum EducationLevel
    {
        [Description("Среднее")]
        HighSchool,
        [Description("Бакалавр")]
        Bachelor,
        [Description("Магистр")]
        Master,
        [Description("PhD")]
        Doctorate
    }

    public class RichDemoObject : ReactiveObject
    {
        private string _fullName = "Иван Петров";
        private int _age = 30;
        private DateTime _birthDate = new DateTime(1995, 5, 15);
        private string _email = "ivan.petrov@example.com";
        private string _phone = "+7 (123) 456-78-90";

        private EmployeeRole _role = EmployeeRole.Developer;
        private WorkShift _shift = WorkShift.Day;
        private double _salary = 75000.50;
        private bool _isFullTime = true;
        private DateTime _hireDate = DateTime.Now.AddYears(-3);
        private DateTime? _terminationDate = null;
        private int _vacationDays = 28;

        private EducationLevel _education = EducationLevel.Master;
        private string _university = "МГУ им. Ломоносова";
        private int _graduationYear = 2018;

        private string _internalNotes = "Это поле скрыто в DataForm";
        private double _bonusPercentage = 15.5;

        [Category("Основные")]
        [DisplayName("Полное имя")]
        public string FullName
        {
            get => _fullName;
            set => this.RaiseAndSetIfChanged(ref _fullName, value);
        }

        [Category("Основные")]
        [DisplayName("Возраст")]
        public int Age
        {
            get => _age;
            set => this.RaiseAndSetIfChanged(ref _age, value);
        }

        [Category("Основные")]
        [DisplayName("Дата рождения")]
        public DateTime BirthDate
        {
            get => _birthDate;
            private set => this.RaiseAndSetIfChanged(ref _birthDate, value);
        }

        [Category("Основные")]
        [DisplayName("Email")]
        public string Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        [Category("Основные")]
        [DisplayName("Телефон")]
        public string Phone
        {
            get => _phone;
            set => this.RaiseAndSetIfChanged(ref _phone, value);
        }

        [Category("Работа")]
        [DisplayName("Должность")]
        public EmployeeRole Role
        {
            get => _role;
            set => this.RaiseAndSetIfChanged(ref _role, value);
        }

        [Category("Работа")]
        [DisplayName("Смена")]
        public WorkShift Shift
        {
            get => _shift;
            set => this.RaiseAndSetIfChanged(ref _shift, value);
        }

        [Category("Работа")]
        [DisplayName("Зарплата")]
        public double Salary
        {
            get => _salary;
            set => this.RaiseAndSetIfChanged(ref _salary, value);
        }

        [Category("Работа")]
        [DisplayName("Полная занятость")]
        public bool IsFullTime
        {
            get => _isFullTime;
            set => this.RaiseAndSetIfChanged(ref _isFullTime, value);
        }

        [Category("Работа")]
        [DisplayName("Дата найма")]
        public DateTime HireDate
        {
            get => _hireDate;
            private set => this.RaiseAndSetIfChanged(ref _hireDate, value);
        }

        [Category("Работа")]
        [DisplayName("Дата увольнения")]
        public DateTime? TerminationDate
        {
            get => _terminationDate;
            set => this.RaiseAndSetIfChanged(ref _terminationDate, value);
        }

        [Category("Работа")]
        [DisplayName("Дней отпуска")]
        public int VacationDays
        {
            get => _vacationDays;
            set => this.RaiseAndSetIfChanged(ref _vacationDays, value);
        }

        [Category("Образование")]
        [DisplayName("Уровень образования")]
        public EducationLevel Education
        {
            get => _education;
            set => this.RaiseAndSetIfChanged(ref _education, value);
        }

        [Category("Образование")]
        [DisplayName("Университет")]
        public string University
        {
            get => _university;
            set => this.RaiseAndSetIfChanged(ref _university, value);
        }

        [Category("Образование")]
        [DisplayName("Год окончания")]
        public int GraduationYear
        {
            get => _graduationYear;
            set => this.RaiseAndSetIfChanged(ref _graduationYear, value);
        }

        [Browsable(false)]
        public string InternalNotes
        {
            get => _internalNotes;
            private set => this.RaiseAndSetIfChanged(ref _internalNotes, value);
        }

        [Browsable(false)]
        public double BonusPercentage
        {
            get => _bonusPercentage;
            set => this.RaiseAndSetIfChanged(ref _bonusPercentage, value);
        }

        public override string ToString() => FullName;
    }
}
