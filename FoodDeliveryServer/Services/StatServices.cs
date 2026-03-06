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
                // 1. 筛选：只要最近 7 天的订单
                .Where(o => o.Order!.OrderDate >= DateTime.Now.AddDays(-7))

                // 3. 分组：把相同名字的菜堆在一起 (比如所有的 Burger 放一堆)
                .GroupBy(o => new { o.FoodId, o.Food!.Name })

                // 4. 统计：对于每一堆(g)，我们要算出什么？
                .Select(g => new BestSellersDto
                {
                    FoodId = g.Key.FoodId,                       // 菜名
                    FoodName = g.Key.Name,
                    TotalSold = g.Sum(o => o.Quantity)  // 卖出的总数量 (把这一堆的 Quantity 加起来)
                })

                // 5. 排序：卖得最多的排在最上面 (Descending = 降序)
                .OrderByDescending(x => x.TotalSold)

                // 6. 截取：只取前 3 名
                .Take(3)

                // 7. 执行查询
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
