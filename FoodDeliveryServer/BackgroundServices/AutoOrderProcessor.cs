using FoodDeliveryServer.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryServer.BackgroundServices
{
    public class AutoOrderProcessor: BackgroundService
    {
        private readonly ILogger<AutoOrderProcessor> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public AutoOrderProcessor(ILogger<AutoOrderProcessor> logger, IServiceScopeFactory serviceFactory)
        {
            _logger = logger;
            _scopeFactory = serviceFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Auto Background Processor is starting.");

            while (!stoppingToken.IsCancellationRequested){
                try
                {
                    await DoWorkAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing Auto Background Processor.");
                }

                _logger.LogInformation("Auto Background Processor is delaying for 10 seconds.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task DoWorkAsync()
        {
            // 👇 Important! Manually create a Scope
            // Similar to manually simulating the lifecycle of an "HTTP Request"
            using (var scope = _scopeFactory.CreateScope())
            {
                // Retrieve the database connection from the Scope (AppDbContext)
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Simple task: count the number of orders
                var count = await context.Orders.CountAsync();

                _logger.LogInformation($" Autocount: {count} orders");

                // --- Future complex logic can be written here ---
                // For example: var expiredOrders = context.Orders.Where(...)
                // context.RemoveRange(expiredOrders);
                // await context.SaveChangesAsync();
            }
            // 👈 Exiting this block destroys the Scope, automatically closing the database connection (disposable)
        }
    }
}
