using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;

namespace MyApiProject.Controllers
{
    [Route("api/fix_ticket_by_manager")]
    [ApiController]
    public class ChangeTicketStatusController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=proiectis.db";
        [HttpPut("{ticketId}")]
        public IActionResult ChangeTicketStatus(int ticketId, [FromBody] UpdateTicketStatusRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username))
            {
                return BadRequest("Invalid request. Please provide all required fields.");
            }
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var checkUserSql = "SELECT user_type FROM Users WHERE username = @username";
                using (var userCommand = new SqliteCommand(checkUserSql, connection))
                {
                    userCommand.Parameters.AddWithValue("@username", request.Username);

                    var userType = userCommand.ExecuteScalar();
                    if (userType == null || userType.ToString() != "Manager")
                    {
                        return Unauthorized("You do not have permission to modify ticket information.");
                    }
                }
                var updateTicketSql = @"
                    UPDATE Tickets
                    SET 
                        ticket_status = @ticket_status,
                        fixed_at = @fixed_at
                    WHERE id_ticket = @ticket_id";

                using (var updateCommand = new SqliteCommand(updateTicketSql, connection))
                {
                    updateCommand.Parameters.AddWithValue("@ticket_id", ticketId);
                    updateCommand.Parameters.AddWithValue("@ticket_status", request.TicketStatus);
                    updateCommand.Parameters.AddWithValue("@fixed_at", request.FixedAt ?? (object)DBNull.Value); // FixedAt poate fi null

                    try
                    {
                        var rowsAffected = updateCommand.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            return NotFound("Ticket not found.");
                        }

                        return Ok("Ticket status updated successfully.");
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, $"Internal server error: {ex.Message}");
                    }
                }
            }
        }
    }
    public class UpdateTicketStatusRequest
    {
        public required string Username { get; set; }
        public required string TicketStatus { get; set; }
        public DateTime? FixedAt { get; set; }
    }
}
