using FoodDeliveryServer.Data;
using FoodDeliveryServer.Dtos;
using FoodDeliveryServer.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryServer.Services
{
    public class StatServices : IStatServices
    {
        private readonly AppDbContext _context;
        public StatServices(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<BestSellersDto>> GetBestSellers()
        {
            var stats = await _context.OrderItems
                // 1. Filter: only orders from the last 7 days
                .Where(o => o.Order!.OrderDate >= DateTime.Now.AddDays(-7))

                // 3. Group: put dishes with the same name together (e.g., all Burgers in one pile)
                .GroupBy(o => new { o.FoodId, o.Food!.Name })

                // 4. Calculate: for each group (g), what do we want to output?
                .Select(g => new BestSellersDto
                {
                    FoodId = g.Key.FoodId,                       // Food ID
                    FoodName = g.Key.Name,
                    TotalSold = g.Sum(o => o.Quantity)  // Total quantity sold (sum of Quantity in this group)
                })

                // 5. Order: the best-selling ones on top (Descending)
                .OrderByDescending(x => x.TotalSold)

                // 6. Limit: only take the top 3
                .Take(3)

                // 7. Execute query
                .ToListAsync();

            return stats;
        }

        public async Task<List<SentimentStatDto>> GetSentimentStats()
        {
            var stats = await _context.Orders
                .GroupBy(o => o.Sentiment)
                .Select(g => new SentimentStatDto
                {
                    Label = g.Key ?? "Unknown",
                    Count = g.Count()
                })
                .ToListAsync();

            return stats;
        }

        public async Task<TopSpenderDto?> GetTopSpender()
        {
            var stats = await _context.Orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .GroupBy(o => o.UserId)
                .Select(g => new TopSpenderDto
                {
                    CustomerId = g.Key,
                    TotalSpent = g.Sum(o => o.TotalPrice)
                })
                .OrderByDescending(x => x.TotalSpent)
                .FirstOrDefaultAsync();
            return stats;
        }
    }
}
