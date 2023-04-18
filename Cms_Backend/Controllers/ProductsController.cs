using Microsoft.AspNetCore.Mvc;
using Cms_Backend.Model;
using MySqlConnector;

namespace Cms_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly string _connectionString;

        public ProductsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/Products/Products
        [HttpGet("Products")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            using MySqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            using MySqlCommand command = new("SELECT * FROM products", connection);

            var products = new List<Product>();
            using MySqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var product = new Product
                {
                    ProductId = reader.GetGuid(0),
                    ProductName = reader.GetString(1),
                    ProductCost = reader.GetDecimal(2)
                };
                products.Add(product);
            }

            return products;
        }
        // GET: api/Products/Orders
        [HttpGet("Orders")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {

            using MySqlConnection connection = new(_connectionString);
            await connection.OpenAsync();
            using MySqlCommand command = new("SELECT * FROM orders", connection);


            var ordersDictionary = new Dictionary<Guid, Order>();
            using MySqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var customerId = reader.GetGuid(0);
                var customerName = reader.GetString(1);
                var productId = reader.GetGuid(2);
                var productName = reader.GetString(3);
                var productCost = reader.GetDecimal(4);
                var quantity = reader.GetInt32(5);

                if (!ordersDictionary.TryGetValue(customerId, out var order))
                {
                    order = new Order
                    {
                        CustomerId = customerId,
                        CustomerName = customerName,
                        Products = new List<OrderItem>()
                    };
                    ordersDictionary[customerId] = order;
                }

                var orderItem = new OrderItem
                {
                    ProductId = productId,
                    ProductName = productName,
                    ProductCost = productCost,
                    Quantity = quantity
                };
                order.Products.Add(orderItem);
            }

            return ordersDictionary.Values.ToList();

        }
    }
}