namespace FoodDeliveryServer.Services
{
    public interface IAIService
    {
        // Input a sentence, return a sentiment result (e.g., "Positive", "Negative", "Neutral")
        Task<string> AnalyzeSentiment(string text);
    }
}
