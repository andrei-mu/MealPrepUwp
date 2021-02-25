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

            var items = userText.Split(new char[] { ',', '=' });
            if (items.Length != 2)
                return;

            if (false == int.TryParse(items[1].Trim(), out var calories))
                return;

            var name = items[0].Trim();
            name = name.ToUpper()[0] + name.Substring(1).ToLower();

            if (false == int.TryParse(IngredientContainerSize.Text, out var containerSize))
                return;
            if (false == int.TryParse(IngredientContainerPrice.Text, out var containerPrice))
                return;

            using (var db = new ApplicationDbContext())
            {
                var ing = new Ingredient()
                {
                    CaloriesPerUnit = calories,
                    Name = name,
                    ContainerSize = containerSize,
                    ContainerPrice = containerPrice,
                    Unit = unit
                };

                db.Ingredients.Add(ing);
                db.SaveChanges();

                UpdateIngredients(db);
            }
        }

        private void IngredientsPage_OnLoaded(Object sender, RoutedEventArgs e)
        {
            using (var db = new ApplicationDbContext())
            {
                UpdateIngredients(db);
            }
        }

        private void BtnMl_OnClick(object sender, RoutedEventArgs e)
        {
            AddIngredient(IngredientUnit.HundredMl);
        }

        private void BtnGrams_OnClick(object sender, RoutedEventArgs e)
        {
            AddIngredient(IngredientUnit.HundredGrams);
        }

        private void BtnPiece_OnClick(object sender, RoutedEventArgs e)
        {
            AddIngredient(IngredientUnit.Piece);
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
    }
}
