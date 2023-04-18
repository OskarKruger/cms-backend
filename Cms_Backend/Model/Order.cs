namespace Cms_Backend.Model
{
    public class Order
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public List<OrderItem> Products { get; set; }
    }

    public class OrderItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductCost { get; set; }
        public int Quantity { get; set; }
    }

}
