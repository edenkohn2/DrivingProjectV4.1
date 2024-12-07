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

        [HttpGet]
        public IActionResult GetInstructors()
        {
            var instructors = new List<Instructor>();

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT p.PersonId, p.FirstName, p.LastName, p.Email, p.PhoneNumber, i.ExperienceYears 
                              FROM person p
                              INNER JOIN instructor i ON p.PersonId = i.InstructorId";
                var command = new MySqlCommand(query, connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        instructors.Add(new Instructor
                        {
                            PersonId = reader.GetInt32("PersonId"),
                            FirstName = reader.GetString("FirstName"),
                            LastName = reader.GetString("LastName"),
                            Email = reader.GetString("Email"),
                            PhoneNumber = reader.GetString("PhoneNumber"),
                            ExperienceYears = reader.GetInt32("ExperienceYears")
                        });
                    }
                }
            }

            return Ok(instructors);
        }
        // כמות שיעורים שמורה עשה לתלמיד
        [HttpGet("{instructorId}/details")]
        public IActionResult GetInstructorDetails(int instructorId)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT COUNT(l.LessonId) AS TotalLessons, COUNT(DISTINCT s.StudentId) AS TotalStudents 
                              FROM lessons l
                              INNER JOIN student s ON l.StudentId = s.StudentId
                              WHERE l.InstructorId = @InstructorId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@InstructorId", instructorId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Ok(new
                        {
                            TotalLessons = reader.GetInt32("TotalLessons"),
                            TotalStudents = reader.GetInt32("TotalStudents")
                        });
                    }
                }
            }

            return NotFound();
        }

        [HttpGet("{instructorId}/students")]
        public IActionResult GetStudentsForInstructor(int instructorId)
        {
            var students = new List<Student>();

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT s.StudentId, p.FirstName, p.LastName, p.Email, p.PhoneNumber 
                              FROM student s
                              INNER JOIN person p ON s.StudentId = p.PersonId
                              INNER JOIN lessons l ON s.StudentId = l.StudentId
                              WHERE l.InstructorId = @InstructorId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@InstructorId", instructorId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        students.Add(new Student
                        {
                            PersonId = reader.GetInt32("StudentId"),
                            FirstName = reader.GetString("FirstName"),
                            LastName = reader.GetString("LastName"),
                            Email = reader.GetString("Email"),
                            PhoneNumber = reader.GetString("PhoneNumber")
                        });
                    }
                }
            }

            return Ok(students);
        }

        [HttpGet("{instructorId}/schedule")]
        public IActionResult GetScheduleForInstructor(int instructorId)
        {
            var schedule = new List<Lesson>();

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT l.LessonId, l.Date, l.Duration, l.LessonType 
                              FROM lessons l
                              WHERE l.InstructorId = @InstructorId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@InstructorId", instructorId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        schedule.Add(new Lesson
                        {
                            LessonId = reader.GetInt32("LessonId"),
                            Date = reader.GetDateTime("Date"),
                            Duration = reader.GetInt32("Duration"),
                            LessonType = reader.GetString("LessonType")
                        });
                    }
                }
            }

            return Ok(schedule);
        }

        [HttpPost("{instructorId}/lessons")]
        public IActionResult AddLessonForInstructor(int instructorId, [FromBody] Lesson lesson)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO lessons (StudentId, InstructorId, Date, Duration, LessonType, Price) 
                              VALUES (@StudentId, @InstructorId, @Date, @Duration, @LessonType, @Price)";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@StudentId", lesson.StudentId);
                command.Parameters.AddWithValue("@InstructorId", instructorId);
                command.Parameters.AddWithValue("@Date", lesson.Date);
                command.Parameters.AddWithValue("@Duration", lesson.Duration);
                command.Parameters.AddWithValue("@LessonType", lesson.LessonType);
                command.Parameters.AddWithValue("@Price", lesson.Price);

                command.ExecuteNonQuery();
            }

            return Ok("Lesson added successfully");
        }
    }
}
