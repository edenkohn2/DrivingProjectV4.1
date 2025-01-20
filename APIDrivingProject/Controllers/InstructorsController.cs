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
        [HttpGet("{instructorId}/details")]
        public IActionResult GetInstructorDetails(int instructorId)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"
            SELECT 
                p.FirstName, 
                p.LastName, 
                p.Email, 
                COUNT(l.LessonId) AS TotalLessons, 
                COUNT(DISTINCT s.StudentId) AS TotalStudents 
            FROM person p
            INNER JOIN instructor i ON p.PersonId = i.InstructorId
            LEFT JOIN lessons l ON l.InstructorId = i.InstructorId
            LEFT JOIN student s ON l.StudentId = s.StudentId
            WHERE i.InstructorId = @InstructorId
            GROUP BY p.FirstName, p.LastName, p.Email";

                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@InstructorId", instructorId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Ok(new
                        {
                            FirstName = reader.GetString("FirstName"),
                            LastName = reader.GetString("LastName"),
                            Email = reader.GetString("Email"),
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
                var query = @"
            SELECT s.StudentId, p.FirstName, p.LastName, p.Email, p.PhoneNumber, s.LessonsTaken
            FROM instructor_student is_map
            INNER JOIN student s ON is_map.StudentId = s.StudentId
            INNER JOIN person p ON s.StudentId = p.PersonId
            WHERE is_map.InstructorId = @InstructorId";
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
                            PhoneNumber = reader.GetString("PhoneNumber"),
                            LessonsTaken = reader.GetInt32("LessonsTaken") // כאן אנו מוודאים שלוקחים את הערך הנכון
                        });
                    }
                }
            }

            return Ok(students);
        }





        [HttpGet("{instructorId}/schedule")]
        public IActionResult GetScheduleForInstructor(int instructorId)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching schedule: {ex.Message}");
            }
        }



        [HttpPost("{instructorId}/lessons")]
        public IActionResult AddLessonForInstructor(int instructorId, [FromBody] Lesson lesson)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();

                    // בדיקת שיוך התלמיד למורה
                    var checkQuery = @"SELECT COUNT(*) 
                               FROM instructor_student 
                               WHERE InstructorId = @InstructorId AND StudentId = @StudentId";
                    var checkCommand = new MySqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@InstructorId", instructorId);
                    checkCommand.Parameters.AddWithValue("@StudentId", lesson.StudentId);

                    var count = Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (count == 0)
                    {
                        return BadRequest("Student is not assigned to this instructor.");
                    }

                    // הוספת שיעור
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

                return Ok("Lesson added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error adding lesson: {ex.Message}");
            }
        }
        [HttpGet("{instructorId}/schedule/today")]
        public IActionResult GetTodayScheduleForInstructor(int instructorId)
        {
            var today = DateTime.Today;
            var schedule = new List<object>();

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT l.LessonId, l.Date, l.Duration, l.LessonType, l.Price, 
                             CONCAT(p.FirstName, ' ', p.LastName) AS StudentName
                      FROM lessons l
                      INNER JOIN student s ON l.StudentId = s.StudentId
                      INNER JOIN person p ON s.StudentId = p.PersonId
                      WHERE l.InstructorId = @InstructorId AND DATE(l.Date) = @Today
                      ORDER BY l.Date";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@InstructorId", instructorId);
                command.Parameters.AddWithValue("@Today", today);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        schedule.Add(new
                        {
                            LessonId = reader.GetInt32("LessonId"),
                            Date = reader.GetDateTime("Date"),
                            Duration = reader.GetInt32("Duration"),
                            LessonType = reader.GetString("LessonType"),
                            Price = reader.GetDecimal("Price"),
                            StudentName = reader.GetString("StudentName")
                        });
                    }
                }
            }

            return Ok(schedule);
        }
        //פונקציה שמאפשרת לתלמיד לשלוח בקשת שיוך למורה
        [HttpPost("request-assignment")]
        public IActionResult RequestAssignment([FromBody] AssignmentRequestModel request)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();

                    // בדוק אם בקשה קיימת כבר
                    var checkQuery = @"SELECT COUNT(*) FROM PendingInstructorStudent 
                               WHERE StudentId = @StudentId AND InstructorId = @InstructorId";
                    var checkCommand = new MySqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@StudentId", request.StudentId);
                    checkCommand.Parameters.AddWithValue("@InstructorId", request.InstructorId);

                    var count = Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (count > 0)
                    {
                        return BadRequest("A request already exists for this student and instructor.");
                    }

                    // הוסף בקשה לטבלת PendingInstructorStudent
                    var query = @"INSERT INTO PendingInstructorStudent (StudentId, InstructorId) 
                          VALUES (@StudentId, @InstructorId)";
                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@StudentId", request.StudentId);
                    command.Parameters.AddWithValue("@InstructorId", request.InstructorId);

                    command.ExecuteNonQuery();
                }

                return Ok("Assignment request sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending assignment request: {ex.Message}");
            }
        }
        //פונקציה שמאפשרת למורה לראות את כל הבקשות הפתוחות לשיוך אליו
        [HttpGet("{instructorId}/pending-requests")]
        public IActionResult GetPendingRequests(int instructorId)
        {
            try
            {
                var requests = new List<PendingRequestModel>();

                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();

                    var query = @"SELECT p.PendingId, p.StudentId, s.FirstName, s.LastName, s.Email, s.PhoneNumber, p.RequestDate 
                          FROM PendingInstructorStudent p
                          INNER JOIN person s ON p.StudentId = s.PersonId
                          WHERE p.InstructorId = @InstructorId";
                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@InstructorId", instructorId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(new PendingRequestModel
                            {
                                PendingId = reader.GetInt32("PendingId"),
                                StudentId = reader.GetInt32("StudentId"),
                                FirstName = reader.GetString("FirstName"),
                                LastName = reader.GetString("LastName"),
                                Email = reader.GetString("Email"),
                                PhoneNumber = reader.GetString("PhoneNumber"),
                                RequestDate = reader.GetDateTime("RequestDate")
                            });
                        }
                    }
                }

                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching pending requests: {ex.Message}");
            }
        }
        //פונקציה שמאפשרת למורה לאשר בקשה ולהעביר את התלמיד לטבלה InstructorStudent
        [HttpPost("{instructorId}/approve-request/{pendingId}")]
        public IActionResult ApproveRequest(int instructorId, int pendingId)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();

                    // שליפת פרטי הבקשה
                    var selectQuery = @"SELECT StudentId FROM PendingInstructorStudent 
                                WHERE PendingId = @PendingId AND InstructorId = @InstructorId";
                    var selectCommand = new MySqlCommand(selectQuery, connection);
                    selectCommand.Parameters.AddWithValue("@PendingId", pendingId);
                    selectCommand.Parameters.AddWithValue("@InstructorId", instructorId);

                    var studentId = (int?)selectCommand.ExecuteScalar();
                    if (studentId == null)
                    {
                        return NotFound("Request not found.");
                    }

                    // הוספת תלמיד לטבלת InstructorStudent
                    var insertQuery = @"INSERT INTO InstructorStudent (InstructorId, StudentId) 
                                VALUES (@InstructorId, @StudentId)";
                    var insertCommand = new MySqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@InstructorId", instructorId);
                    insertCommand.Parameters.AddWithValue("@StudentId", studentId);
                    insertCommand.ExecuteNonQuery();

                    // מחיקת הבקשה מטבלת PendingInstructorStudent
                    var deleteQuery = @"DELETE FROM PendingInstructorStudent WHERE PendingId = @PendingId";
                    var deleteCommand = new MySqlCommand(deleteQuery, connection);
                    deleteCommand.Parameters.AddWithValue("@PendingId", pendingId);
                    deleteCommand.ExecuteNonQuery();
                }

                return Ok("Request approved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error approving request: {ex.Message}");
            }
        }
        //פונקציה שמאפשרת למורה לדחות בקשה:
        [HttpDelete("{instructorId}/reject-request/{pendingId}")]
        public IActionResult RejectRequest(int instructorId, int pendingId)
        {
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();

                    var deleteQuery = @"DELETE FROM PendingInstructorStudent 
                                WHERE PendingId = @PendingId AND InstructorId = @InstructorId";
                    var deleteCommand = new MySqlCommand(deleteQuery, connection);
                    deleteCommand.Parameters.AddWithValue("@PendingId", pendingId);
                    deleteCommand.Parameters.AddWithValue("@InstructorId", instructorId);

                    var rowsAffected = deleteCommand.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return NotFound("Request not found.");
                    }
                }

                return Ok("Request rejected successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error rejecting request: {ex.Message}");
            }
        }
        [HttpGet("{studentId}/request-status")]
        public IActionResult GetRequestStatus(int studentId)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();

                // בדוק אם יש בקשה בתהליך
                var queryPending = @"SELECT i.FirstName, i.LastName, 'Pending' AS Status
                             FROM PendingInstructorStudent p
                             INNER JOIN instructor i ON p.InstructorId = i.InstructorId
                             WHERE p.StudentId = @StudentId";
                var commandPending = new MySqlCommand(queryPending, connection);
                commandPending.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = commandPending.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Ok(new
                        {
                            Status = "Pending",
                            InstructorName = $"{reader.GetString("FirstName")} {reader.GetString("LastName")}"
                        });
                    }
                }

                // בדוק אם התלמיד משויך למורה
                var queryAssigned = @"SELECT i.FirstName, i.LastName, 'Approved' AS Status
                              FROM InstructorStudent a
                              INNER JOIN instructor i ON a.InstructorId = i.InstructorId
                              WHERE a.StudentId = @StudentId";
                var commandAssigned = new MySqlCommand(queryAssigned, connection);
                commandAssigned.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = commandAssigned.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Ok(new
                        {
                            Status = "Approved",
                            InstructorName = $"{reader.GetString("FirstName")} {reader.GetString("LastName")}"
                        });
                    }
                }

                // אם אין נתונים, החזר סטטוס דחייה
                return Ok(new
                {
                    Status = "Rejected",
                    InstructorName = ""
                });
            }
        }
        [HttpGet("{studentId}/is-assigned")]
        public IActionResult IsStudentAssigned(int studentId)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT COUNT(*) 
                      FROM instructor_student 
                      WHERE StudentId = @StudentId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@StudentId", studentId);

                var isAssigned = Convert.ToInt32(command.ExecuteScalar()) > 0;
                return Ok(isAssigned);
            }
        }
        [HttpGet("{studentId}/assignment-status")]
        public IActionResult GetAssignmentStatus(int studentId)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();

                // בדוק אם הבקשה עדיין ממתינה
                // בדיקת בקשה בתהליך
                var pendingQuery = @"SELECT p.FirstName, p.LastName, 'Pending' AS Status
                     FROM pendinginstructorstudent pis
                     INNER JOIN instructor i ON pis.InstructorId = i.InstructorId
                     INNER JOIN person p ON i.InstructorId = p.PersonId
                     WHERE pis.StudentId = @StudentId";
                var pendingCommand = new MySqlCommand(pendingQuery, connection);
                pendingCommand.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = pendingCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Ok(new
                        {
                            Status = "Pending",
                            InstructorName = $"{reader.GetString("FirstName")} {reader.GetString("LastName")}"
                        });
                    }
                }

                // בדיקת שיוך
                var assignedQuery = @"SELECT p.FirstName, p.LastName, 'Approved' AS Status
                      FROM instructor_student ins
                      INNER JOIN instructor i ON ins.InstructorId = i.InstructorId
                      INNER JOIN person p ON i.InstructorId = p.PersonId
                      WHERE ins.StudentId = @StudentId";
                var assignedCommand = new MySqlCommand(assignedQuery, connection);
                assignedCommand.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = assignedCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Ok(new
                        {
                            Status = "Approved",
                            InstructorName = $"{reader.GetString("FirstName")} {reader.GetString("LastName")}"
                        });
                    }
                }

                // לא משויך ולא בבקשה
                return Ok(new
                {
                    Status = "Not Assigned",
                    InstructorName = ""
                });
            }
        }







    }

}

