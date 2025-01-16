using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Data;

namespace MyApiProject.Controllers
{
    [Route("api/create_user")]
    [ApiController]
    public class CreateUserController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=proiectis.db";
        [HttpPost]
        public IActionResult CreateNewUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("User data is null");
            }
            var sql = "INSERT INTO Users (username, user_type) VALUES (@username, @user_type)";
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@user_type", user.UserType);
                    try
                    {
                        command.ExecuteNonQuery();  // Execute the insert query
                        return CreatedAtAction(nameof(CreateNewUser), new { id = user.Username }, user);
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, $"Internal server error: {ex.Message}");
                    }
                }
            }
        }
    }
    public class User
    {
        public required string Username { get; set; }
        public required string UserType { get; set; }
    }
}
