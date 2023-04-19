using Microsoft.AspNetCore.Mvc;
using Cms_Backend.Model;
using MySqlConnector;
using System.Globalization;

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
    }
}