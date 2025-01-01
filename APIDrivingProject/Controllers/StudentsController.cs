using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using APIDrivingProject.Services;
using APIDrivingProject.Models;
using DrivingProjectSharedModels.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIDrivingProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public StudentsController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpPost("register")]
        public IActionResult RegisterStudent([FromBody] Student student)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();

                // הוספת תלמיד לטבלת `person`
                var personQuery = @"INSERT INTO person (FirstName, LastName, Email, PasswordHash, PhoneNumber, Role) 
                                    VALUES (@FirstName, @LastName, @Email, @PasswordHash, @PhoneNumber, 'Student')";
                var personCommand = new MySqlCommand(personQuery, connection);
                personCommand.Parameters.AddWithValue("@FirstName", student.FirstName);
                personCommand.Parameters.AddWithValue("@LastName", student.LastName);
                personCommand.Parameters.AddWithValue("@Email", student.Email);
                personCommand.Parameters.AddWithValue("@PasswordHash", student.PasswordHash);
                personCommand.Parameters.AddWithValue("@PhoneNumber", student.PhoneNumber);
                personCommand.ExecuteNonQuery();

                // קבלת מזהה התלמיד
                var studentId = (int)personCommand.LastInsertedId;

                // הוספת תלמיד לטבלת `student`
                var studentQuery = @"INSERT INTO student (StudentId, LessonsTaken, HasPassedTheory, IsActive) 
                                     VALUES (@StudentId, @LessonsTaken, @HasPassedTheory, @IsActive)";
                var studentCommand = new MySqlCommand(studentQuery, connection);
                studentCommand.Parameters.AddWithValue("@StudentId", studentId);
                studentCommand.Parameters.AddWithValue("@LessonsTaken", student.LessonsTaken);
                studentCommand.Parameters.AddWithValue("@HasPassedTheory", student.HasPassedTheory);
                studentCommand.Parameters.AddWithValue("@IsActive", student.IsActive);
                studentCommand.ExecuteNonQuery();
            }

            return Ok("Student registered successfully");
        }

        [HttpGet]
        public IActionResult GetStudents()
        {
            var students = new List<Student>();

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT s.StudentId, p.FirstName, p.LastName, p.Email, p.PhoneNumber, 
                                     s.LessonsTaken, s.HasPassedTheory, s.IsActive 
                              FROM student s
                              INNER JOIN person p ON s.StudentId = p.PersonId";
                var command = new MySqlCommand(query, connection);

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
                            PhoneNumber = reader.GetString("PhoneNumber"),
                            LessonsTaken = reader.GetInt32("LessonsTaken"),
                            HasPassedTheory = reader.GetBoolean("HasPassedTheory"),
                            IsActive = reader.GetBoolean("IsActive")
                        });
                    }
                }
            }

            return Ok(students);
        }

        [HttpGet("{studentId}")]
        public IActionResult GetStudentDetails(int studentId)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT p.PersonId, p.FirstName, p.LastName, p.Email, p.PhoneNumber, p.BirthDate, 
                                     s.LessonsTaken, s.HasPassedTheory, s.IsActive 
                              FROM student s
                              INNER JOIN person p ON s.StudentId = p.PersonId
                              WHERE s.StudentId = @StudentId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var studentDetails = new
                        {
                            PersonId = reader.GetInt32("PersonId"),
                            FirstName = reader.GetString("FirstName"),
                            LastName = reader.GetString("LastName"),
                            Email = reader.GetString("Email"),
                            PhoneNumber = reader.GetString("PhoneNumber"),
                            BirthDate = reader.GetDateTime("BirthDate"),
                            LessonsTaken = reader.GetInt32("LessonsTaken"),
                            HasPassedTheory = reader.GetBoolean("HasPassedTheory"),
                            IsActive = reader.GetBoolean("IsActive")
                        };

                        return Ok(studentDetails);
                    }
                }
            }

            return NotFound();
        }

        [HttpPut("{studentId}/status")]
        public IActionResult UpdateStudentStatus(int studentId, [FromBody] bool isActive)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE student 
                              SET IsActive = @IsActive 
                              WHERE StudentId = @StudentId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@IsActive", isActive);
                command.Parameters.AddWithValue("@StudentId", studentId);

                command.ExecuteNonQuery();
            }

            return Ok("Student status updated successfully");
        }

        [HttpGet("{studentId}/progress")]
        public IActionResult GetStudentProgress(int studentId)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT LessonsTaken, HasPassedTheory 
                              FROM student 
                              WHERE StudentId = @StudentId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var progress = new
                        {
                            LessonsTaken = reader.GetInt32("LessonsTaken"),
                            HasPassedTheory = reader.GetBoolean("HasPassedTheory")
                        };

                        return Ok(progress);
                    }
                }
            }

            return NotFound();
        }
    }
}
