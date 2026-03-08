using System.Text.Json;

namespace FoodDeliveryServer.Dtos
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;

        // 👇 A handy little method: turning itself into a JSON string
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
