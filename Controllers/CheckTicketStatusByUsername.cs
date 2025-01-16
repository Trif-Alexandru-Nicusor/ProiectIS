using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace MyApiProject.Controllers
{
    [Route("api/check_ticket_status_by_username")]
    [ApiController]
    public class CheckTicketStatusController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=proiectis.db";

        [HttpGet("{username}")]
        public IActionResult GetTicketsByUser(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Username is required.");
            }

            var tickets = new List<object>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var selectTicketsSql = "SELECT id_ticket, ticket_description, ticket_status FROM Tickets WHERE created_by = @username";

                using (var selectCommand = new SqliteCommand(selectTicketsSql, connection))
                {
                    selectCommand.Parameters.AddWithValue("@username", username);

                    using (var reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ticket = new
                            {
                                TicketId = Convert.ToInt32(reader["id_ticket"]),
                                TicketDescription = reader["ticket_description"].ToString(),
                                TicketStatus = reader["ticket_status"].ToString()
                            };
                            tickets.Add(ticket);
                        }
                    }
                }
            }

            if (tickets.Count == 0)
            {
                return NotFound($"No tickets found for user {username}.");
            }

            return Ok(tickets);
        }
    }
}
