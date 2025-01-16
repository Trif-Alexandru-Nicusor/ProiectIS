using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;

namespace MyApiProject.Controllers
{
    [Route("api/create_ticket")]
    [ApiController]
    public class CreateTicketController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=proiectis.db";
        [HttpPost]
        public IActionResult CreateTicket([FromBody] Ticket ticket)
        {
            if (ticket == null || string.IsNullOrEmpty(ticket.TicketDescription) || string.IsNullOrEmpty(ticket.CreatedBy) || string.IsNullOrEmpty(ticket.CategoryName))
            {
                return BadRequest("Invalid ticket data. Please provide ticket_description, created_by, and category_name.");
            }
            int categoryId;
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var selectCategorySql = "SELECT id_category FROM Category WHERE category_name = @category_name";
                using (var selectCommand = new SqliteCommand(selectCategorySql, connection))
                {
                    selectCommand.Parameters.AddWithValue("@category_name", ticket.CategoryName);
                    var result = selectCommand.ExecuteScalar();
                    if (result != null)
                    {
                        categoryId = Convert.ToInt32(result);
                    }
                    else
                    {
                        var insertCategorySql = "INSERT INTO Category (category_name) VALUES (@category_name); SELECT last_insert_rowid();";
                        using (var insertCommand = new SqliteCommand(insertCategorySql, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@category_name", ticket.CategoryName);
                            categoryId = Convert.ToInt32(insertCommand.ExecuteScalar()); // Obține id_category nou creat
                        }
                    }
                }
                var insertTicketSql = @"
                    INSERT INTO Tickets (ticket_description, created_by, created_at, ticket_status, fixed_at, id_category)
                    VALUES (@ticket_description, @created_by, @created_at, @ticket_status, @fixed_at, @id_category)";

                using (var insertCommand = new SqliteCommand(insertTicketSql, connection))
                {
                    insertCommand.Parameters.AddWithValue("@ticket_description", ticket.TicketDescription);
                    insertCommand.Parameters.AddWithValue("@created_by", ticket.CreatedBy);
                    insertCommand.Parameters.AddWithValue("@created_at", DateTime.UtcNow); // Data și ora curente (UTC)
                    insertCommand.Parameters.AddWithValue("@ticket_status", "In Progress"); // Status implicit
                    insertCommand.Parameters.AddWithValue("@fixed_at", DBNull.Value); // Gol implicit
                    insertCommand.Parameters.AddWithValue("@id_category", categoryId); // Referință la categoria asociată

                    try
                    {
                        insertCommand.ExecuteNonQuery();
                        return Ok("Ticket created successfully.");
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, $"Internal server error: {ex.Message}");
                    }
                }
            }
        }
    }

    public class Ticket
    {
        public required string TicketDescription { get; set; }
        public required string CreatedBy { get; set; }
        public required string CategoryName { get; set; }
    }
}
