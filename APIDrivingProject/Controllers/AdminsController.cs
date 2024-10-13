using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using APIDrivingProject.Services;
using APIDrivingProject.Models;
using System.Collections.Generic;
namespace APIDrivingProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public AdminsController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // GET: api/Admins
        [HttpGet]
        public IActionResult GetAdmins()
        {
            var admins = new List<Admin>();

            using (MySqlConnection connection = _databaseService.GetConnection())
            {
                connection.Open();
                string query = "SELECT AdminId, FirstName, LastName, Email, PhoneNumber, Role FROM Admins";
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var admin = new Admin
                    {
                        AdminId = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Email = reader.GetString(3),
                        PhoneNumber = reader.GetString(4),
                        Role = reader.GetString(5)
                    };
                    admins.Add(admin);
                }

                reader.Close();
            }

            return Ok(admins);
        }
    }
}
