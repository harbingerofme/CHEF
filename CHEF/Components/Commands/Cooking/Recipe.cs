﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CHEF.Components.Commands.Cooking
{
    public class RecipeContext : DbContext
    {
        public DbSet<Recipe> Recipes { get; set; }
        public const int NumberPerPage = 25;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Dummy connection string for creating migration
            // through the Package Manager Console with Add-Migration or dotnet ef
            //optionsBuilder.UseNpgsql("Host=dummy;Username=dummy;Password=dummy;Database=dummy");

            optionsBuilder.UseNpgsql(global::CHEF.Database.Connection);
            optionsBuilder.UseSnakeCaseNamingConvention();
        }

        public async Task<Recipe> GetRecipe(string recipeName) =>
            await Recipes.AsQueryable()
                .FirstOrDefaultAsync(r => r.Name.ToLower().Equals(recipeName.ToLower()));

        public async Task<List<Recipe>> GetRecipes(string nameFilter = null, int page = 0)
        {
            List<Recipe> recipes;
            if (nameFilter != null)
            {
                recipes = await Recipes.AsQueryable().Where(r => r.Name.ToLower().Contains(nameFilter.ToLower()))
                    .ToListAsync();
            }
            else
            {
                recipes = await Recipes.AsQueryable().Skip(NumberPerPage * page).Take(NumberPerPage).ToListAsync();
            }

            return recipes;
        }

        public int CountAll()
        {
            return Recipes.AsQueryable().Count();
        }
    }

    public class Recipe
    {
        [Key]
        public int Id { get; set; }
        public ulong OwnerId { get; set; }
        public string OwnerName { get; set; }
        
        public string Name { get; set; }
        public string Text { get; set; }

        public string RealOwnerName(SocketGuild guild)
        {
            var owner = guild.GetUser(OwnerId);
            return owner != null ? owner.ToString() : OwnerName;
        }

        public bool IsOwner(SocketGuildUser user) => OwnerId == user.Id;

        public bool CanEdit(SocketGuildUser user) =>
            IsOwner(user) || user.Roles.Any(role => role.Name.Contains(Roles.CoreDeveloper));
    }
}