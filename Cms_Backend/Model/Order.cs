namespace Cms_Backend.Model
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }
        public decimal Price { get; set; }
        public List<OrderItem> Products { get; set; }
    }

    public class OrderItem
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductCost { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateOrderDto
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
        public List<CreateOrderItemDto> Products { get; set; }
    }

    public class CreateOrderItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductCost { get; set; }
        public int Quantity { get; set; }
    }
}
