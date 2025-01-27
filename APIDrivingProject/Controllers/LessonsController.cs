using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using APIDrivingProject.Services;
using APIDrivingProject.Models;
using DrivingClassLibary.Models;
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

        // Get all lessons
        [HttpGet]
        public IActionResult GetLessons()
        {
            var lessons = ExecuteQuery("SELECT * FROM lessons");
            return Ok(lessons);
        }

        // Get lessons by student
        [HttpGet("student/{studentId}")]
        public IActionResult GetLessonsByStudent(int studentId)
        {
            var lessons = ExecuteQuery("SELECT * FROM lessons WHERE StudentId = @StudentId", ("@StudentId", studentId));
            return Ok(lessons);
        }

        // Get lessons by instructor
        [HttpGet("instructor/{instructorId}")]
        public IActionResult GetLessonsByInstructor(int instructorId)
        {
            var lessons = ExecuteQuery("SELECT * FROM lessons WHERE InstructorId = @InstructorId", ("@InstructorId", instructorId));
            return Ok(lessons);
        }

        // Add a new lesson
        [HttpPost]
        public IActionResult AddLesson([FromBody] Lesson lesson)
        {
            if (lesson == null)
            {
                return BadRequest(new { message = "The lesson field is required." });
            }

            if (lesson.Date == DateTime.MinValue)
            {
                return BadRequest(new { message = "The Date field must be provided in ISO 8601 format (e.g., yyyy-MM-ddTHH:mm:ss)." });
            }

            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = @"INSERT INTO lessons (StudentId, InstructorId, Date, Duration, LessonType, Price) 
                          VALUES (@StudentId, @InstructorId, @Date, @Duration, @LessonType, @Price)";
                    var command = new MySqlCommand(query, connection);
                    AddLessonParameters(command, lesson);
                    command.ExecuteNonQuery();
                }
                return Ok("Lesson added successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while adding the lesson.", details = ex.Message });
            }
        }


        // Update a lesson
        [HttpPut("{lessonId}")]
        public IActionResult UpdateLesson(int lessonId, [FromBody] Lesson updatedLesson)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE lessons 
                              SET StudentId = @StudentId, InstructorId = @InstructorId, Date = @Date, 
                                  Duration = @Duration, LessonType = @LessonType, Price = @Price 
                              WHERE LessonId = @LessonId";
                var command = new MySqlCommand(query, connection);
                AddLessonParameters(command, updatedLesson);
                command.Parameters.AddWithValue("@LessonId", lessonId);
                command.ExecuteNonQuery();
            }

            return Ok("Lesson updated successfully");
        }

        // Delete a lesson
        [HttpDelete("{lessonId}")]
        public IActionResult DeleteLesson(int lessonId)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"DELETE FROM lessons WHERE LessonId = @LessonId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@LessonId", lessonId);
                command.ExecuteNonQuery();
            }

            return Ok("Lesson deleted successfully");
        }

        // Get upcoming lessons for instructor
        [HttpGet("instructor/{instructorId}/upcoming")]
        public IActionResult GetUpcomingLessonsForInstructor(int instructorId)
        {
            var lessons = ExecuteQuery(@"SELECT * FROM lessons 
                                         WHERE InstructorId = @InstructorId AND Date > NOW()",
                                         ("@InstructorId", instructorId));
            return Ok(lessons);
        }

        // Helper: Execute query
        private List<Lesson> ExecuteQuery(string query, params (string paramName, object paramValue)[] parameters)
        {
            var lessons = new List<Lesson>();

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var command = new MySqlCommand(query, connection);

                foreach (var (paramName, paramValue) in parameters)
                {
                    command.Parameters.AddWithValue(paramName, paramValue);
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lessons.Add(new Lesson
                        {
                            LessonId = reader.GetInt32("LessonId"),
                            StudentId = reader.GetInt32("StudentId"),
                            InstructorId = reader.GetInt32("InstructorId"),
                            Date = reader.GetDateTime("Date"),
                            Duration = reader.GetInt32("Duration"),
                            LessonType = reader.GetString("LessonType"),
                            Price = reader.GetDecimal("Price")
                        });
                    }
                }
            }

            return lessons;
        }
        [HttpGet("{lessonId}")]
        public IActionResult GetLessonById(int lessonId)
        {
            try
            {
                Console.WriteLine($"Fetching lesson with ID: {lessonId}");
                var lesson = ExecuteQuery("SELECT * FROM lessons WHERE LessonId = @LessonId", ("@LessonId", lessonId)).FirstOrDefault();
                if (lesson == null)
                {
                    Console.WriteLine("Lesson not found.");
                    return NotFound(new { message = "Lesson not found." });
                }
                Console.WriteLine("Lesson found and returned.");
                return Ok(lesson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving the lesson.", details = ex.Message });
            }
        }



        // Helper: Add parameters for lesson
        private void AddLessonParameters(MySqlCommand command, Lesson lesson)
        {
            command.Parameters.AddWithValue("@StudentId", lesson.StudentId);
            command.Parameters.AddWithValue("@InstructorId", lesson.InstructorId);
            command.Parameters.AddWithValue("@Date", lesson.Date);
            command.Parameters.AddWithValue("@Duration", lesson.Duration);
            command.Parameters.AddWithValue("@LessonType", lesson.LessonType);
            command.Parameters.AddWithValue("@Price", lesson.Price);
        }
    }

}
