namespace FoodDeliveryServer.Dtos
{
    public class BestSellersDto
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
    }
}
