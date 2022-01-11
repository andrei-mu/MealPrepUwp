using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MealPrepUwp.Models
{
    public class WeeklyPlan
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<DailyPlan> DailyPlans { get; set; }

        [NotMapped]
        public string DisplayName
        {
            get
            {
                //return $"{DateTime:M} - {Name}";
                return $"{Name}";
            }
        }

        [NotMapped]
        public int CaloriesPerDay
        {
            get
            {
                return 0; // DailyDishes?.Sum(dd => dd.ServingCalorieCount) ?? 0;
            }
        }
    }
}