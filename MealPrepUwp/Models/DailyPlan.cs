using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MealPrepUwp.Models
{
    public class DailyPlan
    {
        public int Id { get; set; }

        public DayOfWeek Day { get; set; }
        public ICollection<DailyDish> DailyDishes { get; set; }

        public int WeeklyPlanId { get; set; }
        public WeeklyPlan WeeklyPlan { get; set; }

        [NotMapped]
        public string DisplayName
        {
            get
            {
                return $"{Day}";
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