using System.Text.Json;
using IQGame.Client.Models;
using IQGame.Shared.Models;
using Microsoft.JSInterop;

namespace IQGame.Client.Services
{
    public class FavoritesService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string FavoritesKey = "iqgame_favorite_categories";

        public FavoritesService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<List<FavoriteCategory>> GetFavoritesAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", FavoritesKey);
                if (string.IsNullOrEmpty(json))
                    return new List<FavoriteCategory>();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var favorites = JsonSerializer.Deserialize<List<FavoriteCategory>>(json, options);
                return favorites ?? new List<FavoriteCategory>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting favorites: {ex.Message}");
                return new List<FavoriteCategory>();
            }
        }

        public async Task AddToFavoritesAsync(Category category)
        {
            try
            {
                var favorites = await GetFavoritesAsync();
                
                // Check if already exists
                if (!favorites.Any(f => f.Id == category.Id))
                {
                    var favorite = FavoriteCategory.FromCategory(category);
                    favorites.Add(favorite);
                    await SaveFavoritesAsync(favorites);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to favorites: {ex.Message}");
            }
        }

        public async Task AddToFavoritesAsync(SelectableCategory category)
        {
            try
            {
                var favorites = await GetFavoritesAsync();
                
                // Check if already exists
                if (!favorites.Any(f => f.Id == category.Id))
                {
                    var favorite = FavoriteCategory.FromSelectableCategory(category);
                    favorites.Add(favorite);
                    await SaveFavoritesAsync(favorites);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to favorites: {ex.Message}");
            }
        }

        public async Task RemoveFromFavoritesAsync(int categoryId)
        {
            try
            {
                var favorites = await GetFavoritesAsync();
                favorites.RemoveAll(f => f.Id == categoryId);
                await SaveFavoritesAsync(favorites);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from favorites: {ex.Message}");
            }
        }

        public async Task<bool> IsFavoriteAsync(int categoryId)
        {
            try
            {
                var favorites = await GetFavoritesAsync();
                return favorites.Any(f => f.Id == categoryId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if favorite: {ex.Message}");
                return false;
            }
        }

        public async Task ClearFavoritesAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", FavoritesKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing favorites: {ex.Message}");
            }
        }

        private async Task SaveFavoritesAsync(List<FavoriteCategory> favorites)
        {
            try
            {
                var json = JsonSerializer.Serialize(favorites);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", FavoritesKey, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving favorites: {ex.Message}");
            }
        }
    }
} 