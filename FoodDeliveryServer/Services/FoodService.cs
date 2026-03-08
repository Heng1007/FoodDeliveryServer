using FoodDeliveryServer.Data;
using FoodDeliveryServer.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryServer.Services
{
    // 👨‍🍳 This is a Service class
    // It is responsible for the heavy lifting (database operations)
    public class FoodService : IFoodService
    {
        private readonly AppDbContext _context;

        // Only here do we need to use the database!
        public FoodService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<FoodItem>> GetAllFoods()
        {
            return await _context.FoodItems
                         .OrderBy(f => f.Id) // Or f.Name
                         .Where(f => !f.IsDeleted)
                         .ToListAsync();
        }

        public async Task<FoodItem> AddFood(FoodItem food)
        {
            _context.FoodItems.Add(food);
            await _context.SaveChangesAsync();

            return food;
        }

        public async Task DeleteFood(int d)
        {
           var food = await _context.FoodItems.FindAsync(d);
              if (food != null)
              {
                food.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<FoodItem?> GetFoodById(int id)
        {
            return await _context.FoodItems
                .Where(f => !f.IsDeleted)
                .FirstOrDefaultAsync(f => f.Id == id);
        }
    }
}