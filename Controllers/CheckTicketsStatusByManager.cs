using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace MyApiProject.Controllers
{
    [Route("api/check_tickets_status_manager")]
    [ApiController]
    public class CheckTicketsStatusByManagerController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=proiectis.db";

        [HttpGet("{username}")]
        public IActionResult GetTicketsByManager(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Username is required.");
            }

            // Verificăm dacă utilizatorul este manager
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
                            return Unauthorized("You are not authorized to view tickets.");
                        }
                    }
                }

                // Dacă utilizatorul este un manager, returnăm toate biletele
                var tickets = new List<object>();
                var selectTicketsSql = "SELECT id_ticket, ticket_description, ticket_status, created_by FROM Tickets";

                using (var selectCommand = new SqliteCommand(selectTicketsSql, connection))
                {
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ticket = new
                            {
                                TicketId = Convert.ToInt32(reader["id_ticket"]),
                                TicketDescription = reader["ticket_description"].ToString(),
                                TicketStatus = reader["ticket_status"].ToString(),
                                CreatedBy = reader["created_by"].ToString()
                            };
                            tickets.Add(ticket);
                        }
                    }
                }

                if (tickets.Count == 0)
                {
                    return NotFound("No tickets found.");
                }

                return Ok(tickets);
            }
        }
    }
}
