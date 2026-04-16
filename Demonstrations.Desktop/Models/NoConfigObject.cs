using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demonstrations.Desktop.Models
{
    public class NoConfigObject : ReactiveObject
    {
        private string _name = "Объект без настроек";
        private int _count = 100;
        private double _price = 19.99;
        private bool _available = true;
        private DateTime _created = DateTime.Now;
        private string _description = "Описание товара";
        private int _stock = 50;
        private double _discount = 0.05;
        private string _category = "Электроника";
        private bool _featured = false;

        public string Name { get => _name; set => this.RaiseAndSetIfChanged(ref _name, value); }
        public int Count { get => _count; set => this.RaiseAndSetIfChanged(ref _count, value); }
        public double Price { get => _price; set => this.RaiseAndSetIfChanged(ref _price, value); }
        public bool Available { get => _available; set => this.RaiseAndSetIfChanged(ref _available, value); }
        public DateTime Created { get => _created; set => this.RaiseAndSetIfChanged(ref _created, value); }
        public string Description { get => _description; set => this.RaiseAndSetIfChanged(ref _description, value); }
        public int Stock { get => _stock; set => this.RaiseAndSetIfChanged(ref _stock, value); }
        public double Discount { get => _discount; set => this.RaiseAndSetIfChanged(ref _discount, value); }
        public string Category { get => _category; set => this.RaiseAndSetIfChanged(ref _category, value); }
        public bool Featured { get => _featured; set => this.RaiseAndSetIfChanged(ref _featured, value); }
    }
}
