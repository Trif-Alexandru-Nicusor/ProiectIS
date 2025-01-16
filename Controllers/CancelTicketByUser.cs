using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;

namespace MyApiProject.Controllers
{
    [Route("api/cancel_ticket_by_user")]
    [ApiController]
    public class CancelTicketByUserController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=proiectis.db";
        [HttpPut("{ticketId}")]
        public IActionResult CancelTicket(int ticketId, [FromBody] string username)
        {
            if (ticketId <= 0 || string.IsNullOrEmpty(username))
            {
                return BadRequest("Invalid ticket ID or username.");
            }
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var selectTicketSql = "SELECT created_by, ticket_status FROM Tickets WHERE id_ticket = @ticketId";
                using (var selectCommand = new SqliteCommand(selectTicketSql, connection))
                {
                    selectCommand.Parameters.AddWithValue("@ticketId", ticketId);

                    using (var reader = selectCommand.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return NotFound($"Ticket with ID {ticketId} not found.");
                        }
                        var createdBy = reader["created_by"].ToString();
                        var ticketStatus = reader["ticket_status"].ToString();
                        if (createdBy != username)
                        {
                            return Unauthorized("You are not authorized to cancel this ticket.");
                        }
                        if (ticketStatus == "Cancelled")
                        {
                            return BadRequest("This ticket is already cancelled.");
                        }
                    }
                }
                var updateTicketSql = "UPDATE Tickets SET ticket_status = 'Cancelled' WHERE id_ticket = @ticketId";
                using (var updateCommand = new SqliteCommand(updateTicketSql, connection))
                {
                    updateCommand.Parameters.AddWithValue("@ticketId", ticketId);

                    try
                    {
                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok($"Ticket with ID {ticketId} has been successfully cancelled.");
                        }
                        else
                        {
                            return StatusCode(500, "An error occurred while cancelling the ticket.");
                        }
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, $"Internal server error: {ex.Message}");
                    }
                }
            }
        }
    }
}
