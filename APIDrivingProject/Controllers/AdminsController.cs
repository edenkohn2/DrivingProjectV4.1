using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using APIDrivingProject.Services;
using APIDrivingProject.Models;
using DrivingProjectSharedModels.Models;
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
                string query = @"SELECT person.PersonId, person.FirstName, person.LastName, person.Email, person.PhoneNumber, admin.AdminRole
                 FROM person
                 INNER JOIN admin ON person.PersonId = admin.AdminId";

                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var admin = new Admin
                    {
                        PersonId = reader.GetInt32("PersonId"), // שימוש ב-PersonId במקום AdminId
                        FirstName = reader.GetString("FirstName"),
                        LastName = reader.GetString("LastName"),
                        Email = reader.GetString("Email"),
                        PhoneNumber = reader.GetString("PhoneNumber"),
                        AdminRole = reader.GetString("AdminRole")
                    };
                    admins.Add(admin);
                }

                reader.Close();
            }

            return Ok(admins);
        }
    }
}
