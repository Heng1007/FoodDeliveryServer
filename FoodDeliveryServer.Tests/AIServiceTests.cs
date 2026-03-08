using FoodDeliveryServer.Services;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Xunit;

namespace FoodDeliveryServer.Tests
{
    public class AIServiceTests
    {
        private IConfiguration GetMockConfiguration()
        {
            // ❌ Old way: hardcoded (delete this!)
            // var mySettings = new Dictionary<string, string> ... 

            // ✅ New way: tell it to read from "User Secrets"
            // Note: The <AIServiceTests> here is to locate the Secrets ID
            return new ConfigurationBuilder()
                .AddUserSecrets<AIServiceTests>()
                .Build();
        }

        [Fact]
        public async Task AnalyzeSentiment_Should_Return_Positive_For_Good_Text()
        {
            // 1. Arrange
            // We directly instantiate a Service here because the Key is already hardcoded internally
            var config = GetMockConfiguration();
            var service = new AIService(config);
            var input = "The food is amazing! I love it!"; // Test phrase

            // 2. Act (Actually connecting to Azure!)
            var result = await service.AnalyzeSentiment(input);

            // 3. Assert
            // Azure should evaluate this sentence as Positive
            Assert.Equal("Positive", result);
        }

        [Fact]
        public async Task AnalyzeSentiment_Should_Return_Negative_For_Bad_Text()
        {
            var config = GetMockConfiguration();
            var service = new AIService(config);
            var input = "The service is terrible and the food is cold."; // Test phrase: Bad review!

            var result = await service.AnalyzeSentiment(input);

            Assert.Equal("Negative", result);
        }
    }
}