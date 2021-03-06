﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MealPrepUwp.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPrepUwp.Context
{
    sealed class ApplicationDbContext : DbContext
    {
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<DishIngredient> DishIngredients { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DailyDish> DailyDishes { get; set; }
        public DbSet<Models.DailyPlan> DailyPlans { get; set; }

        public ApplicationDbContext()
        {
            if (Database.EnsureCreated())
            {
                PopulateDefaultEntries();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=mealprep.db");
            base.OnConfiguring(optionsBuilder);
        }

        private void PopulateDefaultEntries()
        {
            var ingredients = new Ingredient[]
            {
                new Ingredient() { CaloriesPerUnit = 10, Name = "Rice", ContainerSize = 1, ContainerPrice = 1, Unit = IngredientUnit.HundredGrams},
                new Ingredient() { CaloriesPerUnit = 20, Name = "Meat", ContainerSize = 1, ContainerPrice = 1, Unit = IngredientUnit.HundredMl},
                new Ingredient() { CaloriesPerUnit = 30, Name = "Oil", ContainerSize = 1, ContainerPrice = 1, Unit = IngredientUnit.HundredGrams},
            };

            foreach (var ingredient in ingredients)
            {
                Ingredients.Add(ingredient);
            }

            this.SaveChanges();

            var dishes = new Dish[]
            {
                new Dish()
                {
                    Name = "Ciorba de fasole",
                    ServingsPerDish = 1,
                    ImageUrl = "https://thumbor.unica.ro/unsafe/715x566/smart/filters:contrast(8):quality(80)/https://retete.unica.ro/wp-content/uploads/2010/06/ciorba-de-fasole-1-e1505228989189.jpg"},
            };

            foreach (var dish in dishes)
            {
                Dishes.Add(dish);
            }

            this.SaveChanges();

        }
    }
}
