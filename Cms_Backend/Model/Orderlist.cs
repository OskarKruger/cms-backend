namespace Cms_Backend.Model
{
    public class Orderlist
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public List<OrderItem> Products { get; set; }
        public Orderlist(Guid customerId, string customerName, List<OrderItem> products)
        {
            CustomerId = customerId;
            CustomerName = customerName;
            Products = products;
        }

    }
}
