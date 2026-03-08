using System.Text.Json.Serialization;

namespace FoodDeliveryServer.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int Quantity { get; set; } // How many bought
        public decimal Price { get; set; } // Unit price at the time (prevents price changes from affecting past orders)

        // Relationship: Which main order does it belong to
        public int OrderId { get; set; }

        [JsonIgnore]
        public Order? Order { get; set; }

        // Relationship: Which food item is it
        public int FoodId { get; set; }   // Corresponds to your FoodItem table
        public FoodItem? Food { get; set; }
    }
}