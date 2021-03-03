using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing.Workflow;
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
    public sealed partial class DailyPlanPage : Page
    {
        public DailyPlanPage()
        {
            this.InitializeComponent();
        }

        private DailyPlan SelectedPlan => PlansList.SelectedItem as DailyPlan;
        public Dish SelectedDish { get; set; }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            string name = PlanNameText.Text;
            if (string.IsNullOrWhiteSpace(name))
                return;

            var planDate = DatePicker.SelectedDate?.DateTime ?? DateTime.Today.Date;

            var dailyPlan = new DailyPlan()
            {
                Name = name,
                DateTime = planDate
            };

            using (var db = new ApplicationDbContext())
            {
                db.Add(dailyPlan);

                db.SaveChanges();

            }
            
            RefreshPlansList();
        }

        private void DeletePlanButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedPlan = SelectedPlan;
            if (selectedPlan == null)
                return;

            using (var db = new ApplicationDbContext())
            {
                if (selectedPlan.DailyDishes != null)
                {
                    db.DailyDishes.RemoveRange(selectedPlan.DailyDishes);
                }

                db.Remove(selectedPlan);
                db.SaveChanges();
            }
            
            RefreshPlansList();
        }

        private void RefreshPlansList()
        {
            var id = SelectedPlan?.Id;

            using (var db = new ApplicationDbContext())
            {
                var dailyPlans = db.DailyPlans
                    .OrderBy(x => x.DateTime)
                    .ToArray();

                PlansList.ItemsSource = dailyPlans;

                if (id is int idi)
                {
                    PlansList.SelectedItem = dailyPlans.FirstOrDefault(x => x.Id == idi);
                    RefreshSelectedPlan();
                }
            }
        }

        private void RefreshSelectedPlan()
        {
            var selectedPlan = SelectedPlan;

            if (selectedPlan == null)
                return;

            using (var db = new ApplicationDbContext())
            {
                var plan = db.DailyPlans
                    .Where(x => x.Id == selectedPlan.Id)
                    .Include(x => x.DailyDishes)
                    .ThenInclude(dd => dd.Dish)
                    .ThenInclude(d => d.IngredientQuantities)
                    .ThenInclude(iq => iq.Ingredient).First();

                selectedPlan = plan;
            }
            
            PlanDishesList.ItemsSource = selectedPlan.DailyDishes.OrderBy(x => x.MealType).ToArray();
            CaloriesText.Text = selectedPlan.CaloriesPerDay.ToString();
        }

        private void DailyPlanPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshPlansList();
        }

        private void PlansList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSelectedPlan();
        }


        private void PlanDishesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void SuggestBoxDish_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
                return;

            if (string.IsNullOrWhiteSpace(sender.Text))
                return;

            var filter = sender.Text.ToLower();

            using (var db = new ApplicationDbContext())
            {
                var items = db.Dishes
                                        .Where(x => x.Name.ToLower()
                                        .Contains(filter))
                                        .OrderBy(x => x.Name).Take(10)
                                        .ToArray();

                SuggestBoxDish.ItemsSource = items;
            }
        }

        private void SuggestBoxDish_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is Dish dish)
            {
                sender.Text = dish.DisplayName;
                SelectedDish = dish;
            }
        }

        private void AddDishButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedPlan == null)
                return;

            if (SelectedDish == null)
                return;

            var tod = MealTimeBox.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(tod))
                return;

            MealType mp;
            switch (tod)
            {
                case "Breakfast": mp = MealType.Breakfast;
                    break;
                case "Munch": mp = MealType.Munch;
                    break;
                case "Lunch": mp = MealType.Lunch;
                    break;
                case "Snack": mp = MealType.Snack;
                    break;
                case "Dinner": mp = MealType.Dinner;
                    break;
                default:
                    return;
            }

            var dailyDish = new DailyDish()
            {
                MealType = mp,
                DishId = SelectedDish.Id,
                DailyPlanId = SelectedPlan.Id
            };

            using (var db = new ApplicationDbContext())
            {
                db.Add(dailyDish);
                db.SaveChanges();
            }
            
            RefreshPlansList();
        }

        private void DeleteDishButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dailyDish = PlanDishesList.SelectedItem as DailyDish;
            if (dailyDish == null)
                return;

            using (var db = new ApplicationDbContext())
            {
                db.Remove(dailyDish);
                db.SaveChanges();
            } 
            
            RefreshPlansList();
        }
    }
}
