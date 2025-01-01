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

        [HttpGet]
        public IActionResult GetLessons()
        {
            var lessons = new List<Lesson>();

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT l.LessonId, l.StudentId, l.InstructorId, l.Date, l.Duration, l.LessonType, l.Price 
                              FROM lessons l";
                var command = new MySqlCommand(query, connection);

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

            return Ok(lessons);
        }

        [HttpGet("student/{studentId}")]
        public IActionResult GetLessonsByStudent(int studentId)
        {
            var lessons = new List<Lesson>();

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT l.LessonId, l.Date, l.Duration, l.LessonType, l.Price 
                              FROM lessons l
                              WHERE l.StudentId = @StudentId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lessons.Add(new Lesson
                        {
                            LessonId = reader.GetInt32("LessonId"),
                            Date = reader.GetDateTime("Date"),
                            Duration = reader.GetInt32("Duration"),
                            LessonType = reader.GetString("LessonType"),
                            Price = reader.GetDecimal("Price")
                        });
                    }
                }
            }

            return Ok(lessons);
        }

        [HttpGet("instructor/{instructorId}")]
        public IActionResult GetLessonsByInstructor(int instructorId)
        {
            var lessons = new List<Lesson>();

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT l.LessonId, l.Date, l.Duration, l.LessonType, l.Price 
                              FROM lessons l
                              WHERE l.InstructorId = @InstructorId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@InstructorId", instructorId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lessons.Add(new Lesson
                        {
                            LessonId = reader.GetInt32("LessonId"),
                            Date = reader.GetDateTime("Date"),
                            Duration = reader.GetInt32("Duration"),
                            LessonType = reader.GetString("LessonType"),
                            Price = reader.GetDecimal("Price")
                        });
                    }
                }
            }

            return Ok(lessons);
        }

        [HttpPut("{lessonId}")]
        public IActionResult UpdateLesson(int lessonId, [FromBody] Lesson updatedLesson)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE lessons 
                              SET StudentId = @StudentId, Date = @Date, Duration = @Duration, LessonType = @LessonType, Price = @Price 
                              WHERE LessonId = @LessonId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@StudentId", updatedLesson.StudentId);
                command.Parameters.AddWithValue("@Date", updatedLesson.Date);
                command.Parameters.AddWithValue("@Duration", updatedLesson.Duration);
                command.Parameters.AddWithValue("@LessonType", updatedLesson.LessonType);
                command.Parameters.AddWithValue("@Price", updatedLesson.Price);
                command.Parameters.AddWithValue("@LessonId", lessonId);

                command.ExecuteNonQuery();
            }

            return Ok("Lesson updated successfully");
        }

        [HttpDelete("{lessonId}")]
        public IActionResult DeleteLesson(int lessonId)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"DELETE FROM lessons 
                              WHERE LessonId = @LessonId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@LessonId", lessonId);

                command.ExecuteNonQuery();
            }

            return Ok("Lesson deleted successfully");
        }

        [HttpGet("instructor/{instructorId}/upcoming")]
        public IActionResult GetUpcomingLessonsForInstructor(int instructorId)
        {
            var lessons = new List<Lesson>();

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT l.LessonId, l.Date, l.Duration, l.LessonType 
                              FROM lessons l
                              WHERE l.InstructorId = @InstructorId AND l.Date > NOW()";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@InstructorId", instructorId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lessons.Add(new Lesson
                        {
                            LessonId = reader.GetInt32("LessonId"),
                            Date = reader.GetDateTime("Date"),
                            Duration = reader.GetInt32("Duration"),
                            LessonType = reader.GetString("LessonType")
                        });
                    }
                }
            }

            return Ok(lessons);
        }
    }
}
