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

        private WeeklyPlan SelectedWeeklyPlan => PlansList.SelectedItem as WeeklyPlan;
        private DailyPlan SelectedDailyPlan => DailyPlans.SelectedItem as DailyPlan;
        public Dish SelectedDish { get; set; }

        private void AddDailyPlans(WeeklyPlan weeklyPlan, ApplicationDbContext db)
        {
            foreach(DayOfWeek dow in Enum.GetValues(typeof(DayOfWeek)))
            {
                DailyPlan dailyPlan = new DailyPlan()
                {
                    Day = dow,
                    WeeklyPlanId = weeklyPlan.Id
                };

                db.Add(dailyPlan);
                db.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"# DEBUG # Created new daily plan with id={dailyPlan.Id}");
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            string name = PlanNameText.Text;
            if (string.IsNullOrWhiteSpace(name))
                return;

            int personCount = 0;
            if (false == int.TryParse(this.PlanPersonCountText.Text, out personCount))
            {
                return;
            }



            var weeklyPlan = new WeeklyPlan()
            {
                Name = name,
                PersonCount = personCount
            };

            using (var db = new ApplicationDbContext())
            {
                db.Add(weeklyPlan);

                db.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"# DEBUG # Created new Weekly plan [{weeklyPlan.Name}] with id={weeklyPlan.Id}");

                AddDailyPlans(weeklyPlan, db);
            }
            
            RefreshWeeklyPlansList();
        }

        private void DeletePlanButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedPlan = SelectedWeeklyPlan;
            if (selectedPlan == null)
                return;

            using (var db = new ApplicationDbContext())
            {
                foreach (var dp in selectedPlan.DailyPlans)
                {
                    db.DailyDishes.RemoveRange(dp.DailyDishes);
                }

                db.DailyPlans.RemoveRange(selectedPlan.DailyPlans);
                db.Remove(selectedPlan);
                db.SaveChanges();
            }
            
            RefreshWeeklyPlansList();
        }

        private void RefreshWeeklyPlansList()
        {
            var wid = SelectedWeeklyPlan?.Id;
            var did = SelectedDailyPlan?.Id;

            using (var db = new ApplicationDbContext())
            {
                var weeklyPlans = db.WeeklyPlans
                    .Include(x => x.DailyPlans)
                    .ThenInclude(dd => dd.DailyDishes)
                    .OrderBy(x => x.Name)
                    .ToArray();

                PlansList.ItemsSource = weeklyPlans;

                if (wid is int idi)
                {
                    PlansList.SelectedItem = weeklyPlans.FirstOrDefault(x => x.Id == idi);
                    RefreshSelectedWeeklyPlan();
                }
            }

            if (!did.HasValue)
            {
                did = SelectedWeeklyPlan?.DailyPlans.First()?.Id;
            }

            RefreshDailyPlan(did);
        }

        private void RefreshDailyPlan(int? planId)
        {
            if (!planId.HasValue)
            {
                return;
            }

            using (var db = new ApplicationDbContext())
            {
                var plan = db.DailyPlans
                    .Where(x => x.Id == planId)
                    .Include(x => x.DailyDishes)
                    .ThenInclude(dd => dd.Dish)
                    .ThenInclude(d => d.DishIngredients)
                    .ThenInclude(iq => iq.Ingredient)
                    .First();

                var selectedPlan = plan;
                DailyPlans.SelectedItem = plan;

                PlanDishesList.ItemsSource = selectedPlan.DailyDishes.OrderBy(x => x.MealType).ToArray();

                int personCount = SelectedWeeklyPlan.PersonCount;
                CaloriesText.Text = (selectedPlan.CaloriesPerDay/personCount).ToString();
            }
        }

        private void RefreshSelectedWeeklyPlan()
        {
            var selectedWeeklyPlan = SelectedWeeklyPlan;
            var did = SelectedDailyPlan?.Id;

            if (selectedWeeklyPlan == null)
            {
                DailyPlans.ItemsSource = null;
                PlanDishesList.ItemsSource = null;

                return;
            }

            using (var db = new ApplicationDbContext())
            {
                var plan = db.WeeklyPlans
                    .Where(x => x.Id == selectedWeeklyPlan.Id)
                    .Include(x => x.DailyPlans).First();

                var dailyPlans = plan.DailyPlans.OrderBy(x => x.Day).ToArray();
                DailyPlans.ItemsSource = dailyPlans;

                var prevPlan = dailyPlans.FirstOrDefault(x => x.Id == did);

                if (prevPlan == null)
                {
                    this.DailyPlans.SelectedItem = dailyPlans[0];
                    did = dailyPlans[0].Id;
                }
                else
                {
                    this.DailyPlans.SelectedItem = prevPlan;
                    did = prevPlan.Id;
                }
            }

            RefreshDailyPlan(did);
        }

        private void DailyPlanPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshWeeklyPlansList();
        }

        private void PlansList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSelectedWeeklyPlan();
        }


        private void PlanDishesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            return;
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
            if (SelectedDailyPlan == null)
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
                DailyPlanId = SelectedDailyPlan.Id
            };

            using (var db = new ApplicationDbContext())
            {
                db.Add(dailyDish);
                db.SaveChanges();
            }
            
            RefreshSelectedWeeklyPlan();
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
            
            RefreshSelectedWeeklyPlan();
        }

        private void DailyPlans_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshDailyPlan(SelectedDailyPlan?.Id);
        }
    }
}
