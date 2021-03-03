using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MealPrepUwp.Context;
using MealPrepUwp.Models;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MealPrepUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IngredientsPage : Page
    {
        public IngredientsPage()
        {
            this.InitializeComponent();
        }

        private void UpdateIngredients(ApplicationDbContext db)
        {
            IngredientsList.ItemsSource = db.Ingredients.OrderBy(x => x.Name).ToList();
        }

        private void AddIngredient(IngredientUnit unit)
        {
            var userText = IngredientText.Text;
            if (string.IsNullOrWhiteSpace(userText))
                return;

            var name = userText.Trim();
            name = name.ToUpper()[0] + name.Substring(1).ToLower();

            if (false == int.TryParse(IngredientContainerSize.Text, out var containerSize))
            {
                containerSize = 1;
            }

            if (false == int.TryParse(IngredientContainerPrice.Text, out var containerPrice))
            {
                containerPrice = 0;
            }

            if (false == int.TryParse(IngredientCaloriesText.Text, out var calorieCount))
                return;

            using (var db = new ApplicationDbContext())
            {
                var ing = new Ingredient()
                {
                    Name = name,
                    CaloriesPerUnit = calorieCount,
                    ContainerSize = containerSize,
                    ContainerPrice = containerPrice,
                    Unit = unit
                };

                db.Ingredients.Add(ing);
                db.SaveChanges();

                UpdateIngredients(db);
            }

            IngredientCaloriesText.Text = String.Empty;
        }

        private void IngredientsPage_OnLoaded(Object sender, RoutedEventArgs e)
        {
            using (var db = new ApplicationDbContext())
            {
                UpdateIngredients(db);
            }
        }

        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            using (var db = new ApplicationDbContext())
            {
                if (IngredientsList.SelectedItem is Ingredient ingredient)
                {
                    db.Ingredients.Remove(ingredient);
                    db.SaveChanges();
                    UpdateIngredients(db);
                }
            }
        }

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            var tod = IngredientUnit.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(tod))
                return;

            switch (tod)
            {
                case "100ml":
                    AddIngredient(Models.IngredientUnit.HundredMl);
                    return;
                case "100 grams":
                    AddIngredient(Models.IngredientUnit.HundredGrams);
                    return;
                case "Bucata":
                    AddIngredient(Models.IngredientUnit.Piece);
                    return;
                case "Felie":
                    AddIngredient(Models.IngredientUnit.Slice);
                    return;
                case "Sticla":
                    AddIngredient(Models.IngredientUnit.Bottle);
                    return;
                default:
                    throw new Exception("Invalid unit");
            }
        }

        private void IngredientText_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
                return;

            if (string.IsNullOrWhiteSpace(sender.Text))
                return;

            var filter = sender.Text.ToLower();
            using (var db = new ApplicationDbContext())
            {
                var suggestion = db.Ingredients
                    .Where(x => x.Name.ToLower().Contains(filter))
                    .OrderBy(x => x.Name.ToLower().IndexOf(filter))
                    .ThenBy(x => x.Name)
                    .Take(10)
                    .Select(x => x.Name)
                    .ToArray();

                sender.ItemsSource = suggestion;
            }
        }
    }
}
