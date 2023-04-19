using Cms_Backend.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace Cms_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly string _connectionString;

        public OrdersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        // GET: api/Products/Orders
        [HttpGet("Orders")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            using MySqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            // Update the SQL queries to match the new schema
            using MySqlCommand command = new(@"SELECT o.id, o.customer_id, o.customer_name, o.status, o.date, o.price,
                                     oi.order_id, oi.product_id, oi.product_name, oi.product_cost, oi.quantity
                                     FROM orders o
                                     JOIN order_items oi ON o.id = oi.order_id
                                     ORDER BY o.customer_id ASC", connection);

            var ordersDictionary = new Dictionary<Guid, Order>();
            using MySqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var Id = reader.GetGuid(0);
                var customerId = reader.GetGuid(1);
                var customerName = reader.GetString(2);
                var status = reader.GetString(3);
                var date = reader.GetDateTime(4).ToString("dd-MM-yyyy HH:mm");
                var price = reader.GetDecimal(5);
                var productId = reader.GetGuid(7);
                var productName = reader.GetString(8);
                var productCost = reader.GetDecimal(9);
                var quantity = reader.GetInt32(10);

                if (!ordersDictionary.TryGetValue(Id, out var order))
                {
                    order = new Order
                    {
                        Id = Id,
                        CustomerId = customerId,
                        CustomerName = customerName,
                        Status = status,
                        Date = date,
                        Price = price,
                        Products = new List<OrderItem>()
                    };
                    ordersDictionary[Id] = order;
                }

                var orderItem = new OrderItem
                {
                    OrderId = Id,
                    ProductId = productId,
                    ProductName = productName,
                    ProductCost = productCost,
                    Quantity = quantity
                };
                order.Products.Add(orderItem);
            }

            return ordersDictionary.Values.OrderByDescending(o => o.Date).ToList();
        }
        [HttpPost("CreateOrders")]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderDto orderData)
        {
            using MySqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var newOrderId = Guid.NewGuid();

                // Insert order data into the orders table
                using MySqlCommand insertOrderCommand = new(@"INSERT INTO orders (id, customer_id, customer_name, status, date, price)
                                                      VALUES (@id, @customer_id, @customer_name, @status, @date, @price)", connection);
                insertOrderCommand.Transaction = transaction; // Associate the command with the transaction
                insertOrderCommand.Parameters.AddWithValue("@id", newOrderId);
                insertOrderCommand.Parameters.AddWithValue("@customer_id", orderData.CustomerId);
                insertOrderCommand.Parameters.AddWithValue("@customer_name", orderData.CustomerName);
                insertOrderCommand.Parameters.AddWithValue("@status", orderData.Status);
                insertOrderCommand.Parameters.AddWithValue("@date", DateTime.Now);
                insertOrderCommand.Parameters.AddWithValue("@price", orderData.Price);

                await insertOrderCommand.ExecuteNonQueryAsync();

                // Insert order items into the order_items table
                foreach (var product in orderData.Products)
                {
                    using MySqlCommand insertOrderItemCommand = new(@"INSERT INTO order_items (order_id, product_id, product_name, product_cost, quantity)
                                                   VALUES (@order_id, @product_id, @product_name, @product_cost, @quantity)", connection);
                    insertOrderItemCommand.Transaction = transaction; // Associate the command with the transaction
                    insertOrderItemCommand.Parameters.AddWithValue("@order_id", newOrderId);
                    insertOrderItemCommand.Parameters.AddWithValue("@product_id", product.ProductId);
                    insertOrderItemCommand.Parameters.AddWithValue("@product_name", product.ProductName);
                    insertOrderItemCommand.Parameters.AddWithValue("@product_cost", product.ProductCost);
                    insertOrderItemCommand.Parameters.AddWithValue("@quantity", product.Quantity);

                    await insertOrderItemCommand.ExecuteNonQueryAsync();

                }

                await transaction.CommitAsync();

                return CreatedAtAction("GetOrders", new { id = newOrderId }, orderData);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"An error occurred while creating the order: {ex.Message}");
            }

        }
        //DELETE: api/Orders/DeleteOrders/5
        [HttpDelete("DeleteOrders/{id}")]
        public async Task<ActionResult<Order>> DeleteOrder(Guid id)
        {
            using MySqlConnection connection = new(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                using MySqlCommand deleteOrderCommand = new(@"DELETE FROM orders WHERE id = @id", connection);
                deleteOrderCommand.Transaction = transaction; // Associate the command with the transaction
                deleteOrderCommand.Parameters.AddWithValue("@id", id);
                await deleteOrderCommand.ExecuteNonQueryAsync();
                using MySqlCommand deleteOrderItemCommand = new(@"DELETE FROM order_items WHERE order_id = @order_id", connection);
                deleteOrderItemCommand.Transaction = transaction; // Associate the command with the transaction
                deleteOrderItemCommand.Parameters.AddWithValue("@order_id", id);
                await deleteOrderItemCommand.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"An error occurred while deleting the order: {ex.Message}");
            }
        }
    }
}
