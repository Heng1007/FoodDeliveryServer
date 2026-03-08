namespace FoodDeliveryServer.Models
{
    public class Order
    {
        public int Id { get; set; }

        // If you have a User table, it's better to store UserId; otherwise, keep CustomerName for now
        public int UserId { get; set; }
        // public string CustomerName { get; set; } // (Old, recommend changing to UserId)

        public DateTime OrderDate { get; set; } = DateTime.Now;

        // 👇 Fix: Price must be a stored static number, not calculated via get
        public decimal TotalPrice { get; set; }

        // 👇 New: Customer note
        public string? CustomerNote { get; set; }

        // 👇 New: AI analysis result
        public string? Sentiment { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // 👇 Crucial: An order contains a bunch of specific items
        public List<OrderItem> Items { get; set; } = new();
    }
}