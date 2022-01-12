using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPrepUwp.Models
{
    public class Dish
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ServingsPerDish { get; set; }

        public string DishUrl { get; set; }

        public string DishNotes{ get; set; }
        public virtual ICollection<DishIngredient> DishIngredients { get; set; }

        [NotMapped]
        public int CalorieCount
        {
            get
            {
                return DishIngredients?.Sum(x => (int)(x.Quantity * x.Ingredient.CaloriesPerUnit + 0.5)) ?? 0;
            }
        }

        [NotMapped]
        public int CaloriePerServingCount => (int)((float)CalorieCount / (float)ServingsPerDish + 0.5);

        [NotMapped] public string DisplayName => Name;
    }
}
