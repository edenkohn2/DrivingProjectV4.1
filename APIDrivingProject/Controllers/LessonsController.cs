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
                            Price = reader.GetDecimal("Price"),
                            StudentName = reader.GetString("StudentName") // הוסף זאת!
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
        [HttpGet("instructor/{instructorId}/date/{date}")]
        public IActionResult GetLessonsByInstructorAndDate(int instructorId, string date)
        {
            var lessons = ExecuteQuery(@"
        SELECT l.LessonId, l.StudentId, l.InstructorId, l.Date, l.Duration, l.LessonType, l.Price,
               CONCAT(p.FirstName, ' ', p.LastName) AS StudentName
        FROM lessons l
        INNER JOIN student s ON l.StudentId = s.StudentId
        INNER JOIN person p ON s.StudentId = p.PersonId
        WHERE l.InstructorId = @InstructorId AND DATE(l.Date) = @Date",
                ("@InstructorId", instructorId),
                ("@Date", date));

            return Ok(lessons);
        }

        public static bool HasColumn(MySqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        [HttpGet("instructor/{instructorId}/week/{startOfWeek}/{endOfWeek}")]
        public IActionResult GetLessonsByInstructorAndWeek(int instructorId, string startOfWeek, string endOfWeek)
        {
            var lessons = ExecuteQuery(@"
        SELECT l.LessonId, l.StudentId, l.InstructorId, l.Date, l.Duration, l.LessonType, l.Price, 
               CONCAT(p.FirstName, ' ', p.LastName) AS StudentName
        FROM lessons l
        INNER JOIN student s ON l.StudentId = s.StudentId
        INNER JOIN person p ON s.StudentId = p.PersonId
        WHERE l.InstructorId = @InstructorId 
        AND DATE(l.Date) BETWEEN @StartOfWeek AND @EndOfWeek",
                ("@InstructorId", instructorId),
                ("@StartOfWeek", startOfWeek),
                ("@EndOfWeek", endOfWeek)
            );

            return Ok(lessons);
        }

        // בודק אם השיעור הסתיים (תאריך + משך < Now), אם כן -> מסמן Completed, יוצר תשלום Pending
        [HttpPut("{lessonId}/check")]
        public IActionResult CheckIfLessonEndedAndComplete(int lessonId)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();

                // שליפת נתוני השיעור
                var selectLessonQuery = @"
            SELECT StudentId, Date, Duration, Price, Status
            FROM lessons
            WHERE LessonId = @LessonId
        ";
                using var selectCmd = new MySqlCommand(selectLessonQuery, connection);
                selectCmd.Parameters.AddWithValue("@LessonId", lessonId);

                int studentId = 0;
                DateTime startTime = DateTime.MinValue;
                int duration = 0;
                decimal price = 0;
                string lessonStatus = "";
                bool found = false;

                using (var reader = selectCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        found = true;
                        studentId = reader.GetInt32("StudentId");
                        startTime = reader.GetDateTime("Date");
                        duration = reader.GetInt32("Duration");
                        price = reader.GetDecimal("Price");
                        lessonStatus = reader.GetString("Status");
                    }
                }
                if (!found)
                {
                    return NotFound("Lesson not found");
                }
                if (lessonStatus == "Completed" || lessonStatus == "Canceled")
                {
                    return BadRequest("Lesson is already completed or canceled.");
                }

                // חישוב זמן סיום השיעור
                DateTime endTime = startTime.AddMinutes(duration);

                if (DateTime.Now < endTime)
                {
                    return BadRequest($"Lesson not ended yet. It ends at {endTime}.");
                }

                // עדכון השיעור ל-Completed
                var updateLessonQuery = "UPDATE lessons SET Status='Completed' WHERE LessonId=@LessonId";
                using var updateLessonCmd = new MySqlCommand(updateLessonQuery, connection);
                updateLessonCmd.Parameters.AddWithValue("@LessonId", lessonId);
                updateLessonCmd.ExecuteNonQuery();

                // יצירת רשומת תשלום עם סטטוס 'Pending'
                var insertPaymentQuery = @"
            INSERT INTO payments (StudentId, Amount, PaymentDate, PaymentMethod, Description, Status)
            VALUES (@StudentId, @Amount, @PaymentDate, 'Pending', @Desc, 'Pending')
        ";
                using var insertPayCmd = new MySqlCommand(insertPaymentQuery, connection);
                insertPayCmd.Parameters.AddWithValue("@StudentId", studentId);
                insertPayCmd.Parameters.AddWithValue("@Amount", price);
                insertPayCmd.Parameters.AddWithValue("@PaymentDate", DateTime.Now);
                insertPayCmd.Parameters.AddWithValue("@Desc", $"Payment for lesson #{lessonId}");
                insertPayCmd.ExecuteNonQuery();

                return Ok($"Lesson #{lessonId} marked as Completed, Payment created (Pending).");
            }
        }


        // שאר ה-CRUD (AddLesson, DeleteLesson, וכו') בהתאם להגדרות הקיימות...





    }

}
