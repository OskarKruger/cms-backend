namespace Cms_Backend.Model
{
    public class Customer
    {
        public string Id { get; set; }
        public string CustomerName { get; set; }
        public Guid CustomerId { get; set; }
    }
    public class CreateCustomerDto
    {
        public string CustomerName { get; set; }
    }
}
