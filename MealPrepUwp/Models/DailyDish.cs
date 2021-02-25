using System.ComponentModel.DataAnnotations.Schema;

namespace MealPrepUwp.Models
{
    public enum MealType
    {
        Breakfast,
        Munch,
        Lunch,
        Snack,
        Dinner,
    }

    public class DailyDish
    {
        public int Id { get; set; }

        public MealType MealType { get; set; }

        public int DishId { get; set; }
        public Dish Dish { get; set; }

        public int DailyPlanId { get; set; }
        public DailyPlan DailyPlan { get; set; }

        [NotMapped]
        public string MealDisplayName
        {
            get
            {
                return MealType.ToString();
            }
        }
        [NotMapped]
        public string DisplayName
        {
            get
            {
                if (Dish == null)
                    return "<unknown>";

                return $"{MealDisplayName} - {Dish.DisplayName}";
            }
        }

        [NotMapped]
        public int CalorieCount => Dish?.CalorieCount ?? 0;

        [NotMapped]
        public int ServingCalorieCount => Dish?.CaloriePerServingCount ?? 0;

        [NotMapped]
        public string CalorieCountDisplay
        {
            get
            {
                return $"{CalorieCount} calories";
            }
        }

        [NotMapped]
        public string ServingCalorieCountDisplay
        {
            get
            {
                return $"{ServingCalorieCount} calories per serving";
            }
        }
    }
}
