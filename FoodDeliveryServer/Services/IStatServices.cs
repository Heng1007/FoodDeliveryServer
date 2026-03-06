using FoodDeliveryServer.Dtos;

namespace FoodDeliveryServer.Services
{
    public interface IStatServices
    {
        Task<List<BestSellersDto>> GetBestSellers();
        Task<TopSpenderDto?> GetTopSpender();
        Task<List<SentimentStatDto>> GetSentimentStats();
    }
}
