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
using Microsoft.EntityFrameworkCore;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MealPrepUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DishesPage : Page
    {
        public DishesPage()
        {
            this.InitializeComponent();
        }

        private void UpdateDishes(ApplicationDbContext db)
        {
            DishesList.ItemsSource = db.Dishes.OrderBy(x => x.Name).Include(x => x.IngredientQuantities).ToList();

            UpdateSelectedDish(db);
        }

        private void UpdateSelectedDish(ApplicationDbContext db)
        {
            var selectedDish = GetSelectedDish();

            if (selectedDish == null)
            {
                DishesIngredientsList.ItemsSource = null;
                return;
            }

            var dish = db.Dishes
                .Where(x => x.Id == selectedDish.Id)
                .Include(d => d.IngredientQuantities)
                .ThenInclude(iq => iq.Ingredient)
                .FirstOrDefault();

            DishesIngredientsList.ItemsSource = dish?.IngredientQuantities;

            TotalCaloriesText.Text = dish?.CalorieCount.ToString() ?? "0";
            ServingCaloresText.Text = dish?.CaloriePerServingCount.ToString() ?? "0";

        }

        private void DishesPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            using (var db = new ApplicationDbContext())
            {
                UpdateDishes(db);
            }
        }

        private void AddDish_OnClick(object sender, RoutedEventArgs e)
        {
            var dishName = DishNameText.Text;
            if (string.IsNullOrWhiteSpace(dishName))
                return;

            if (false == int.TryParse(DishServings.Text, out int servings))
                return;

            if (servings <= 0)
                return;

            var dish = new Dish()
            {
                Name = dishName,
                ServingsPerDish = servings
            };

            using (var db = new ApplicationDbContext())
            {
                db.Dishes.Add(dish);
                db.SaveChanges();

                UpdateDishes(db);
            }
        }

        private void DelDish_OnClick(object sender, RoutedEventArgs e)
        {
            var dish = GetSelectedDish();
            if (dish == null)
            {
                return;
            }

            using (var db = new ApplicationDbContext())
            {
                db.DishIngredients.RemoveRange(dish.IngredientQuantities);
                db.Remove(dish);

                db.SaveChanges();

                UpdateDishes(db);
            }
        }

        private void ChangeDish_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DishesIngredientsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void DishesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (var db = new ApplicationDbContext())
            {
                UpdateSelectedDish(db);
            }
        }

        private void BoxIngredients_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
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

        private void BoxIngredients_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            using (var db = new ApplicationDbContext())
            {
                var selection = args.ChosenSuggestion as string;
                var ingredient = GetSelectedIngredient(selection.ToLower(), db);

                UnitName.Text = ingredient?.UnitName ?? "<none>";
            }
        }


        private void BtnAddIngredient_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedDish = GetSelectedDish();
            if (selectedDish == null)
                return;

            if (!float.TryParse(IngredientUnits.Text, out float unitCount))
                return;

            var textIngredient = BoxIngredients.Text.ToLower();

            using (var db = new ApplicationDbContext())
            {
                var ingredient = GetSelectedIngredient(textIngredient, db);
                if (ingredient == null)
                    return;

                var dishIngredients = new DishIngredient()
                {
                    Quantity = unitCount,
                    DishId = selectedDish.Id,
                    IngredientId = ingredient.Id
                };

                db.DishIngredients.Add(dishIngredients);
                db.SaveChanges();

                UpdateSelectedDish(db);
            }
        }

        private void BtnDelIngredient_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Ingredient GetSelectedIngredient(string text, ApplicationDbContext db)
        {
            return db.Ingredients.FirstOrDefault(x => x.Name.ToLower() == text);
        }

        private Dish GetSelectedDish()
        {
            return DishesList.SelectedItem as Dish;
        }
    }
}
