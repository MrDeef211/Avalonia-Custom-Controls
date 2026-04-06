using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace Common.Controls.Models
{
    public class PieChartGenerator
    {
        public PieChartDataBase GenerateDefaultData()
        {
            var db = new PieChartDataBase();
            db.Add("Категория A", 35);
            db.Add("Категория B", 25);
            db.Add("Категория C", 20);
            db.Add("Категория D", 15);
            db.Add("Категория E", 5);
            return db;
        }

        public PieChartDataBase GenerateRandomData()
        {
            var rand = new Random();
            var db = new PieChartDataBase();
            int sectorsCount = rand.Next(3, 8);
            for (int i = 0; i < sectorsCount; i++)
            {
                db.Add($"Сектор {i + 1}", rand.Next(1, 50));
            }
            return db;
        }
    }
}
