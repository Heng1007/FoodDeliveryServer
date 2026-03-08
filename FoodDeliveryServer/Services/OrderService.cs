using FoodDeliveryServer.Data;
using FoodDeliveryServer.Dtos;
using FoodDeliveryServer.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryServer.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IAIService _aiService;

        public OrderService(AppDbContext context, IAIService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<PagedResult<Order>> GetOrdersAsync(int page, int pageSize)
        {
            var totalCount = await _context.Orders.CountAsync();
            var skipAmount = (page - 1) * pageSize;

            var items = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Food)
                .OrderByDescending(o => o.OrderDate)
                .Skip(skipAmount)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Order>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<List<Order>> GetOrders()
        {

            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Food)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByUserId(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items).ThenInclude(i => i.Food).ToListAsync();
        }

        public async Task<Order> CreateOrder(int userId, CreateOrderDto dto)
        {
            decimal totalPrice = 0;
            var finalOrderItems = new List<OrderItem>();

            foreach(var itemDto in dto.Items)
            {
                var food = await _context.FoodItems.FindAsync(itemDto.FoodItemId);

                if (food == null) continue;

                totalPrice += food.Price * itemDto.Quantity;

                var orderItem = new OrderItem
                {
                    FoodId = food.Id,
                    Price = food.Price,
                    Quantity = itemDto.Quantity,
                };

                finalOrderItems.Add(orderItem);
            }

            string sentimentResult = "Neutral";
            if (!string.IsNullOrEmpty(dto.CustomerNote))
            {
                // If the customer wrote a note, send it to AI for analysis
                sentimentResult = await _aiService.AnalyzeSentiment(dto.CustomerNote);
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                CustomerNote = dto.CustomerNote,
                TotalPrice = totalPrice,
                Sentiment = sentimentResult,
                Items = finalOrderItems,
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task<TopSpenderDto?> GetTopSpender()
        {
            var topSpender = await _context.Orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .GroupBy(o => o.UserId)
                .Select(g => new TopSpenderDto
                {
                    CustomerId = g.Key,
                    TotalSpent = g.Sum(o => o.TotalPrice)
                })
                .OrderByDescending(x => x.TotalSpent)
                .FirstOrDefaultAsync(); // 👈 Taking the first one directly, returns null if none found
            return topSpender;
        }

        public async Task SetOrderStatus(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return;

            order.Status = status;

            await _context.SaveChangesAsync();
        }

        public async Task<string?> CancelOrder(int orderId, int userId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return "Order not found.";
            }

            if(order.UserId != userId)
            {
                return "You do not have permission to cancel this order.";
            }

            if(order.Status != OrderStatus.Pending)
            {
                return "Only pending orders can be cancelled.";
            }

            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();
            return null;

        }

        public async Task<decimal> GetTotalRevenue()
        {
            return await _context.Orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalPrice);
        }
    }

}
