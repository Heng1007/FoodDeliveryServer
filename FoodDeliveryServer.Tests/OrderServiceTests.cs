using FoodDeliveryServer.Data;
using FoodDeliveryServer.Models;
using FoodDeliveryServer.Services;
using Microsoft.EntityFrameworkCore;
using Moq; // 👈 Mocking tool
using Xunit; // 👈 Testing framework

namespace FoodDeliveryServer.Tests
{
    public class OrderServiceTests
    {
        // This is a helper method to create a "fake database"
        // Before each test, we want a clean memory database
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Give each test a unique name to prevent conflicts
                .Options;
            return new AppDbContext(options);
        }

        [Fact] // 👈 Tells the system this is a test
        public async Task CancelOrder_ShouldSucceed_WhenOrderIsPendingAndUserIsOwner()
        {
            // —————— 1. Arrange ——————

            // A. Prepare fake database
            var context = GetInMemoryDbContext();

            // B. Insert a "mock order" into the fake database
            var orderId = 1;
            var userId = 100;
            context.Orders.Add(new Order
            {
                Id = orderId,
                UserId = userId,       // This is the owner
                Status = OrderStatus.Pending, // Status is Pending which can be cancelled
                TotalPrice = 50
            });
            await context.SaveChangesAsync(); // Save fake data

            // C. Prepare the stand-in for AI service (Because OrderService needs IAIService)
            var mockAiService = new Mock<IAIService>();

            // D. Create the main subject we want to test: OrderService
            var service = new OrderService(context, mockAiService.Object);

            // —————— 2. Act ——————
            // Try to cancel the order
            var result = await service.CancelOrder(orderId, userId);

            // —————— 3. Assert ——————

            // Validation A: Return value should be null (representing success, no error message)
            Assert.Null(result);

            // Validation B: Check the database again to confirm the status really changed to Cancelled
            var updatedOrder = await context.Orders.FindAsync(orderId);
            Assert.Equal(OrderStatus.Cancelled, updatedOrder!.Status);
        }

        [Fact]
        public async Task CancelOrder_ShouldFail_WhenUserIsNotOwner()
        {
            // —————— Arrange ——————
            var context = GetInMemoryDbContext();

            // We only need to define that the order's owner is No. 1
            int ownerId = 1;
            int hackerId = 2; // Hacker ID

            context.Orders.Add(new Order
            {
                Id = 999,
                UserId = ownerId, // 👈 Ownership: No. 1
                Status = OrderStatus.Pending,
                TotalPrice = 100
            });
            await context.SaveChangesAsync();

            var mockIAiService = new Mock<IAIService>();
            var orderService = new OrderService(context, mockIAiService.Object);

            // —————— Action ——————
            // ❌ Delete User 1's action, do not let him touch this order! Keep the order in Pending status.

            // Only let the hacker try
            var result = await orderService.CancelOrder(999, hackerId);

            // —————— Assert ——————
            // 1. Verification must report an error (cannot be null)
            Assert.NotNull(result);

            // 2. Verification error message must be accurate
            // ⚠️ Warning: Copy and paste that error report from your OrderService.cs, not a single punctuation mark can be wrong!
            // Assuming the code says "You do not have permission to cancel this order."
            Assert.Equal("You do not have permission to cancel this order.", result);
        }

        [Fact]
        public async Task CancelOrder_ShouldFail_WhenOrderIsAlreadyDelivered()
        {
            var context = GetInMemoryDbContext();
            var ownerId = 1;
            var deliveredOrder = new Order
            {
                Id = 999,
                UserId = ownerId,
                Status = OrderStatus.Delivered, // Already delivered
                TotalPrice = 100
            };

            var mockAIService = new Mock<IAIService>();
            var orderService = new OrderService(context, mockAIService.Object);

            context.Orders.Add(deliveredOrder); // Add order

            await context.SaveChangesAsync();

            var CancelResult = await orderService.CancelOrder(999, ownerId);

            Assert.NotNull(CancelResult);
            Assert.Equal("Only pending orders can be cancelled.", CancelResult);
        }
    }
}