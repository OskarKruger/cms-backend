using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cms_Backend;
using Cms_Backend.Model;
using MySqlConnector;
using System.Configuration;
using MySqlConnector;

namespace Cms_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly string _connectionString;

        public CustomersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/Customers
        [HttpGet("Customers")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            using MySqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            // SQL query to select all customers
            using MySqlCommand command = new(@"SELECT id, customer_name, customer_id
                                      FROM customers
                                      ORDER BY customer_id ASC", connection);

            var customers = new List<Customer>();
            using MySqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var id = reader.GetString(0);
                var customerName = reader.GetString(1);
                var customerId = reader.GetGuid(2);

                var customer = new Customer
                {
                    Id = id,
                    CustomerName = customerName,
                    CustomerId = customerId
                };

                customers.Add(customer);
            }

            return customers;
        }


        // POST: api/Customers
        [HttpPost("CreateCustomers")]
        public async Task<ActionResult<Customer>> CreateCustomer([FromBody] CreateCustomerDto customerData)
        {
            using MySqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var newCustomerId = Guid.NewGuid();
                var newCustomerStringId = Guid.NewGuid().ToString();

                // Insert customer data into the customers table
                using MySqlCommand insertCustomerCommand = new(@"INSERT INTO customers (id, customer_name, customer_id)
                                                      VALUES (@id, @customer_name, @customer_id)", connection);
                insertCustomerCommand.Transaction = transaction; // Associate the command with the transaction
                insertCustomerCommand.Parameters.AddWithValue("@id", newCustomerStringId);
                insertCustomerCommand.Parameters.AddWithValue("@customer_name", customerData.CustomerName);
                insertCustomerCommand.Parameters.AddWithValue("@customer_id", newCustomerId);

                await insertCustomerCommand.ExecuteNonQueryAsync();

                await transaction.CommitAsync();

                var createdCustomer = new Customer
                {
                    Id = newCustomerStringId,
                    CustomerName = customerData.CustomerName,
                    CustomerId = newCustomerId
                };

                return CreatedAtAction("GetCustomers", new { id = newCustomerStringId }, createdCustomer);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"An error occurred while creating the customer: {ex.Message}");
            }
        }
        // DELETE: api/Customers/5
        [HttpDelete("DeleteCustomers/{id}")]
        public async Task<ActionResult<Customer>> DeleteCustomer(string id)
        {
            using MySqlConnection connection = new(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Delete customer data from the customers table
                using MySqlCommand deleteCustomerCommand = new(@"DELETE FROM customers
                                                      WHERE id = @id", connection);
                deleteCustomerCommand.Transaction = transaction; // Associate the command with the transaction
                deleteCustomerCommand.Parameters.AddWithValue("@id", id);
                await deleteCustomerCommand.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"An error occurred while deleting the customer: {ex.Message}");
            }
        }

    }
}
