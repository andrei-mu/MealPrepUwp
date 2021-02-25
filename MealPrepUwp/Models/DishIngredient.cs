using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPrepUwp.Models
{
    public class DishIngredient
    {
        public int Id { get; set; }
        public float Quantity { get; set; }
        public int DishId { get; set; }
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }

        [NotMapped]
        public string DisplayName
        {
            get
            {
                if (Ingredient == null)
                    return "<unknown>";

                var calories = (int) (Quantity * Ingredient.CaloriesPerUnit + 0.5);
                return $"{Quantity} * {Ingredient.Name} = {calories} calories";
            }
        }
    }
}
