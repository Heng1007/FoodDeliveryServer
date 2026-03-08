using Xunit;
using Microsoft.EntityFrameworkCore;
using FoodDeliveryServer.Data;
using FoodDeliveryServer.Services;
using FoodDeliveryServer.Models;
using System.Threading.Tasks;
using System;

namespace FoodDeliveryServer.Tests
{
    public class FoodServiceTests
    {
        // 🛠 1. Prepare Tools: Create an "InMemory" database
        // A new database name is generated every run to ensure tests do not interfere with each other
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact] // 👈 This tag tells the system: This is a test case
        public async Task AddFood_Should_Save_Food_To_Database()
        {
            // 🔶 A1: Arrange 
            var context = GetInMemoryDbContext(); // Get the fake database
            var service = new FoodService(context); // Give the fake database to the Service

            var newFood = new FoodItem
            {
                Name = "Test Nasi Lemak",
                Price = 12.50m
            };

            // 🔶 A2: Act 
            // Actually add the food
            await service.AddFood(newFood);

            // 🔶 A3: Assert
            // Check the database:

            // Check 1: Did the quantity in the database become 1?
            var count = await context.FoodItems.CountAsync();
            Assert.Equal(1, count);

            // Check 2: Is the name of the food correct?
            var savedFood = await context.FoodItems.FirstAsync();
            Assert.Equal("Test Nasi Lemak", savedFood.Name);
        }
    }
}