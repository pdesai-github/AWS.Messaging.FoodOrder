namespace FoodOrder.Producer.Models
{
    public class FoodOrderRequest
    {
        public string OrderId { get; set; } = Guid.NewGuid().ToString();
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public Dictionary<string, List<string>> MessageAttributes { get; set; } = new();
        public string MessageGroupId { get; set; } = "food-orders";
    }

    public class OrderItem
    {
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
