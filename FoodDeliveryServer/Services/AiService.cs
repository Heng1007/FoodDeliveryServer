using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Configuration;

namespace FoodDeliveryServer.Services
{
    public class AIService : IAIService
    {
        private readonly string _endpoint;
        private readonly string _key;
        private readonly TextAnalyticsClient _client;

        public AIService(IConfiguration configuration)
        {
            _endpoint = configuration["AzureSettings:Endpoint"]
                ?? throw new InvalidOperationException("Azure Endpoint is missing!");

            _key = configuration["AzureSettings:Key"]
                   ?? throw new InvalidOperationException("Azure Key is missing!");

            // Initialize the client (make the call)
            var credentials = new AzureKeyCredential(_key);
            var endpointUri = new Uri(_endpoint);
            _client = new TextAnalyticsClient(endpointUri, credentials);
        }

        public async Task<string> AnalyzeSentiment(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "Neutral";

            try
            {
                // 1. Call Azure AI (the cloud interaction happens in an instant here)
                DocumentSentiment result = await _client.AnalyzeSentimentAsync(text);

                // 2. Get the result (Positive, Negative, Neutral, Mixed)
                return result.Sentiment.ToString();
            }
            catch (Exception ex)
            {
                // If offline or the Key is wrong, return a default value to prevent crash
                Console.WriteLine($"AI Error: {ex.Message}");
                return "Error";
            }
        }
    }
}