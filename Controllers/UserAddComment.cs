using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;
namespace MyApiProject.Controllers
{
    [Route("api/add_comment_to_ticket")]
    [ApiController]
    public class UserAddCommentToTicketController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=proiectis.db";

        [HttpPost]
        public IActionResult AddCommentToTicket([FromBody] AddCommentRequest request)
        {
            if (request == null || request.TicketId <= 0 || string.IsNullOrEmpty(request.Comment) || string.IsNullOrEmpty(request.Username))
            {
                return BadRequest("Invalid data. Please provide ticket_id, username, and comment.");
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var selectUserSql = "SELECT COUNT(*) FROM Users WHERE username = @username";
                using (var selectCommand = new SqliteCommand(selectUserSql, connection))
                {
                    selectCommand.Parameters.AddWithValue("@username", request.Username);
                    var userExists = Convert.ToInt32(selectCommand.ExecuteScalar()) > 0;

                    if (!userExists)
                    {
                        return NotFound($"User with username {request.Username} not found.");
                    }
                }
                var selectTicketSql = "SELECT COUNT(*) FROM Tickets WHERE id_ticket = @ticketId";
                using (var selectCommand = new SqliteCommand(selectTicketSql, connection))
                {
                    selectCommand.Parameters.AddWithValue("@ticketId", request.TicketId);
                    var ticketExists = Convert.ToInt32(selectCommand.ExecuteScalar()) > 0;

                    if (!ticketExists)
                    {
                        return NotFound($"Ticket with ID {request.TicketId} not found.");
                    }
                }
                var updateTicketSql = @"
                    UPDATE Tickets 
                    SET ticket_comment = @ticketComment 
                    WHERE id_ticket = @ticketId";

                using (var updateCommand = new SqliteCommand(updateTicketSql, connection))
                {
                    updateCommand.Parameters.AddWithValue("@ticketId", request.TicketId);
                    updateCommand.Parameters.AddWithValue("@ticketComment", request.Comment);

                    try
                    {
                        int rowsAffected = updateCommand.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            return NotFound($"Ticket with ID {request.TicketId} not found.");
                        }

                        return Ok("Comment added successfully.");
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, $"Internal server error: {ex.Message}");
                    }
                }
            }
        }
    }
    public class AddCommentRequest
    {
        public int TicketId { get; set; }
        public string Username { get; set; }
        public string Comment { get; set; }
    }
}
