using System.Text.Json.Serialization;

namespace FoodDeliveryServer.Models
{

    // 👇 This Attribute is crucial! It tells Swagger to convert 0,1,2 into "Pending", "InProgress"...
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderStatus
    {
        Pending,      // Pending
        InProgress,   // In Progress
        Delivered,    // Delivered
        Cancelled     // Cancelled
    }
}