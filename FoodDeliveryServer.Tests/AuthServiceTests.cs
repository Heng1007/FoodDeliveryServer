using FoodDeliveryServer.Data;
using FoodDeliveryServer.Dtos;
using FoodDeliveryServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FoodDeliveryServer.Tests
{
    public class AuthServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Give each test a unique name to prevent conflicts
                .Options;
            return new AppDbContext(options);
        }


        [Fact]
        public async Task Register_ShouldFail_WhenUsernameAlreadyExists()
        {
            // —————— Arrange ——————
            var context = GetInMemoryDbContext();

            // 1. Prepare user data
            var userDto = new UserDto
            {
                Username = "Heng",
                Password = "123456"
            };

            // 2. Prepare Configuration (Dependency)
            // Because AuthService needs to read "Jwt:Key" when generating Token internally
            var mockConfig = new Mock<IConfiguration>();
            // Tell the fake Config: If someone asks for "Jwt:Key", give them "super_secret_key_123456789"
            mockConfig.Setup(c => c["MyJwtKey"]).Returns("super_secret_key_123456789_must_be_long_enough");

            // 3. Prepare fake Logger (Parameter 3) 👈 New addition!
            // The <AuthService> here tells Moq: I want to mock the Logger dedicated to AuthService
            var mockLogger = new Mock<ILogger<AuthService>>();

            // 3. Create AuthService
            // Note: We don't need Mock<IAuthService> here because we are testing it!
            // Assuming your constructor is (DbContext, IConfiguration)
            var service = new AuthService(context, mockConfig.Object, mockLogger.Object);

            // —————— Act ——————

            // Action 1: First registration (Should succeed)
            var successResult = await service.Register(userDto);

            // Action 2: Register again with the same name (Should fail)
            var failResult = await service.Register(userDto);

            // —————— Assert ——————

            // Validation 1: First time should succeed (Returns Token, not null)
            Assert.NotNull(successResult);
            // The judging logic here depends on how your AuthService is written
            // If successful, returns Token (long string), if failed, returns error message (short string)
            Assert.True(successResult.Length > 50); // Token is usually very long

            // Validation 2: Second time should fail (Returns error message)
            // 🚨 Keep in mind: We need to check failResult, not successResult
            Assert.Equal("Username already exists.", failResult);
        }

    }
}
