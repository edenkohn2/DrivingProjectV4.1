using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using APIDrivingProject.Services;
using APIDrivingProject.Models;
using System.Collections.Generic;
namespace APIDrivingProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonsController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public LessonsController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // GET: api/Lessons
        [HttpGet]
        public IActionResult GetLessons()
        {
            var lessons = new List<Lesson>();

            using (MySqlConnection connection = _databaseService.GetConnection())
            {
                connection.Open();
                string query = "SELECT LessonId, StudentId, InstructorId, Date, Duration, LessonType, Price FROM Lessons";
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var lesson = new Lesson
                    {
                        LessonId = reader.GetInt32(0),
                        StudentId = reader.GetInt32(1),
                        InstructorId = reader.GetInt32(2),
                        Date = reader.GetDateTime(3),
                        Duration = reader.GetInt32(4),
                        LessonType = reader.GetString(5),
                        Price = reader.GetDecimal(6)
                    };
                    lessons.Add(lesson);
                }

                reader.Close();
            }

            return Ok(lessons);
        }
    }
}
