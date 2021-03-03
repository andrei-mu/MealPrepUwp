using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPrepUwp.Models
{
    public enum IngredientUnit
    {
        HundredGrams,
        HundredMl,
        Piece,
        Slice,
        Bottle
    };

    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IngredientUnit Unit { get; set; }
        public int CaloriesPerUnit { get; set; }
        public int ContainerSize { get; set; }
        public int ContainerPrice { get; set; }

        [NotMapped]
        public string DisplayName
        {
            get
            {
                return $"{Name} = {CaloriesPerUnit} cal / {UnitName}";
            }
        }

        [NotMapped]
        public string UnitName
        {
            get
            {
                switch (Unit)
                {
                    case IngredientUnit.Piece:
                        return "piece";
                    case IngredientUnit.Slice:
                        return "slice";
                    case IngredientUnit.HundredGrams:
                        return "100 gram";
                    case IngredientUnit.HundredMl:
                        return "100 ml";
                    default:
                        throw new Exception("invalid unit");
                }
            }
        }
    }
}
