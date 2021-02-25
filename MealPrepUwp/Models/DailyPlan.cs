using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MealPrepUwp.Models
{
    public class DailyPlan
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime DateTime { get; set; }

        public ICollection<DailyDish> DailyDishes { get; set; }

        [NotMapped]
        public string DisplayName
        {
            get
            {
                return $"{DateTime:M} - {Name}";
            }
        }

        [NotMapped]
        public int CaloriesPerDay
        {
            get
            {
                return DailyDishes?.Sum(dd => dd.ServingCalorieCount) ?? 0;
            }
        }
    }
}