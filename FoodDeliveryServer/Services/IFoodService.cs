using FoodDeliveryServer.Models;

namespace FoodDeliveryServer.Services
{
    // 📜 This is an Interface
    // It only specifies "what we need to do", not "how to do it"
    public interface IFoodService
    {
        // 1. Get all food items
        Task<List<FoodItem>> GetAllFoods();

        // 2. Add food item
        Task<FoodItem> AddFood(FoodItem food);

        // 4. Delete food item
        Task DeleteFood(int id);

        // 3. Find food by ID (for use with Order)
        Task<FoodItem?> GetFoodById(int id);
    }
}