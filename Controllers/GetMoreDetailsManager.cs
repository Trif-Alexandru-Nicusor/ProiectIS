using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;

namespace MyApiProject.Controllers
{
    [Route("api/get_more_details_manager")]
    [ApiController]
    public class GetTicketDetailsByManagerController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=proiectis.db";
        [HttpGet("{username}/{ticketId}")]
        public IActionResult GetTicketDetails(string username, int ticketId)
        {
            if (string.IsNullOrEmpty(username) || ticketId <= 0)
            {
                return BadRequest("Invalid username or ticket ID.");
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var selectUserSql = "SELECT user_type FROM Users WHERE username = @username";
                using (var selectCommand = new SqliteCommand(selectUserSql, connection))
                {
                    selectCommand.Parameters.AddWithValue("@username", username);
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return NotFound($"User {username} not found.");
                        }

                        var userType = reader["user_type"].ToString();
                        if (userType != "Manager")
                        {
                            return Unauthorized("You are not authorized to view ticket details.");
                        }
                    }
                }
                var selectTicketSql = @"
                    SELECT t.id_ticket, t.ticket_description, t.ticket_status, t.created_by, t.created_at, 
                           t.fixed_at, t.id_category, c.category_name
                    FROM Tickets t
                    INNER JOIN Category c ON t.id_category = c.id_category
                    WHERE t.id_ticket = @ticketId";

                using (var selectCommand = new SqliteCommand(selectTicketSql, connection))
                {
                    selectCommand.Parameters.AddWithValue("@ticketId", ticketId);

                    using (var reader = selectCommand.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return NotFound($"Ticket with ID {ticketId} not found.");
                        }
                        var ticketDetails = new
                        {
                            TicketId = Convert.ToInt32(reader["id_ticket"]),
                            TicketDescription = reader["ticket_description"].ToString(),
                            TicketStatus = reader["ticket_status"].ToString(),
                            CreatedBy = reader["created_by"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["created_at"]).ToString("yyyy-MM-dd HH:mm:ss"),
                            FixedAt = reader["fixed_at"] != DBNull.Value ? Convert.ToDateTime(reader["fixed_at"]).ToString("yyyy-MM-dd HH:mm:ss") : null,
                            CategoryName = reader["category_name"].ToString()
                        };
                        return Ok(ticketDetails);
                    }
                }
            }
        }
    }
}
