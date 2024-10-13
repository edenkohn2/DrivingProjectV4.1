using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using APIDrivingProject.Services;
using APIDrivingProject.Models;
using System.Collections.Generic;
namespace APIDrivingProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorsController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public InstructorsController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // GET: api/Instructors
        [HttpGet]
        public IActionResult GetInstructors()
        {
            var instructors = new List<Instructor>();

            using (MySqlConnection connection = _databaseService.GetConnection())
            {
                connection.Open();
                string query = "SELECT InstructorId, FirstName, LastName, Email, PhoneNumber, ExperienceYears FROM Instructors";
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var instructor = new Instructor
                    {
                        InstructorId = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Email = reader.GetString(3),
                        PhoneNumber = reader.GetString(4),
                        ExperienceYears = reader.GetInt32(5)
                    };
                    instructors.Add(instructor);
                }

                reader.Close();
            }

            return Ok(instructors);
        }
    }
}
