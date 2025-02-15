using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using APIDrivingProject.Services;
using DrivingClassLibary.Models;
using System.Collections.Generic;

namespace APIDrivingProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public PaymentsController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }


        // Endpoint לקבלת תשלומים – לפני השליפה מתבצעת בדיקה ועדכון של שיעורים שעברו את זמנם
        [HttpGet]
        public IActionResult GetPayments()
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();

                // קודם: בדיקה ועדכון שיעורים שסיימו את זמן השיעור
                UpdateEndedLessons(connection);

                // כעת: שליפת רשומות התשלום
                var payments = new List<Payment>();
                var query = @"
                    SELECT 
                        p.PaymentId,
                        p.StudentId,
                        p.Amount,
                        p.PaymentDate,
                        p.PaymentMethod,
                        p.Description,
                        p.Status,
                        pr.FirstName,
                        pr.LastName
                    FROM payments p
                    JOIN student s ON s.StudentId = p.StudentId
                    JOIN person pr ON pr.PersonId = s.StudentId
                ";
                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        payments.Add(new Payment
                        {
                            PaymentId = reader.GetInt32("PaymentId"),
                            StudentId = reader.GetInt32("StudentId"),
                            Amount = reader.GetDecimal("Amount"),
                            PaymentDate = reader.GetDateTime("PaymentDate"),
                            PaymentMethod = reader.GetString("PaymentMethod"),
                            Description = reader.GetString("Description"),
                            Status = reader.GetString("Status"),
                            StudentFullName = reader.GetString("FirstName") + " " + reader.GetString("LastName")
                        });
                    }
                }

                return Ok(payments);
            }
        }
        // מתודה זו בודקת אילו שיעורים עדיין לא סומנו כ־Completed/Cancelled ועברו את זמן הסיום שלהם,
        // ומבצעת עבורם עדכון של סטטוס השיעור ויצירת רשומת תשלום חדשה.
        private void UpdateEndedLessons(MySqlConnection connection)
        {
            Console.WriteLine("[DEBUG] Starting UpdateEndedLessons...");
            Console.WriteLine($"[DEBUG] NOW() = {DateTime.Now}");


            // שליפת שיעורים שסיימו את זמנם ושאינם סומנו כ-Completed או Canceled
            var selectQuery = @"
        SELECT LessonId, StudentId, InstructorId, Date, Duration, Price, Status
        FROM lessons 
        WHERE Status NOT IN ('Completed', 'Canceled')
          AND DATE_ADD(Date, INTERVAL Duration MINUTE) <= NOW()
    ";

            var lessonsToUpdate = new List<(int LessonId, int StudentId, int InstructorId, DateTime Date, int Duration, decimal Price)>();

            try
            {
                using (var selectCmd = new MySqlCommand(selectQuery, connection))
                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int lessonId = reader.GetInt32("LessonId");
                        int studentId = reader.GetInt32("StudentId");
                        int instructorId = reader.GetInt32("InstructorId");
                        DateTime lessonDate = reader.GetDateTime("Date");
                        int duration = reader.GetInt32("Duration");
                        decimal price = reader.GetDecimal("Price");
                        string lessonStatus = reader.GetString("Status");

                        DateTime computedEndTime = lessonDate.AddMinutes(duration);
                        Console.WriteLine($"[DEBUG] Found lesson: LessonId={lessonId}, StudentId={studentId}, InstructorId={instructorId}, Date={lessonDate}, Duration={duration}, Price={price}, Status={lessonStatus}, ComputedEndTime={computedEndTime}");

                        lessonsToUpdate.Add((lessonId, studentId, instructorId, lessonDate, duration, price));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DEBUG] Error executing select query in UpdateEndedLessons: " + ex.Message);
            }

            // עבור כל שיעור שעבר את זמנו – עדכון סטטוס ויצירת תשלום
            foreach (var lesson in lessonsToUpdate)
            {
                try
                {
                    Console.WriteLine($"[DEBUG] Updating lesson status for LessonId={lesson.LessonId}");
                    var updateQuery = "UPDATE lessons SET Status='Completed' WHERE LessonId=@LessonId";
                    using (var updateCmd = new MySqlCommand(updateQuery, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@LessonId", lesson.LessonId);
                        int rowsAffected = updateCmd.ExecuteNonQuery();
                        Console.WriteLine($"[DEBUG] Lesson update affected {rowsAffected} row(s) for LessonId={lesson.LessonId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Error updating lesson status for LessonId={lesson.LessonId}: {ex.Message}");
                }

                try
                {
                    Console.WriteLine($"[DEBUG] Inserting payment for LessonId={lesson.LessonId}");
                    var insertQuery = @"
                INSERT INTO payments (StudentId, InstructorId, Amount, PaymentDate, PaymentMethod, Description, Status)
                VALUES (@StudentId, @InstructorId, @Amount, NOW(), 'CreditCard', @Description, 'Pending')
            ";
                    using (var insertCmd = new MySqlCommand(insertQuery, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@StudentId", lesson.StudentId);
                        insertCmd.Parameters.AddWithValue("@InstructorId", lesson.InstructorId);
                        insertCmd.Parameters.AddWithValue("@Amount", lesson.Price);
                        insertCmd.Parameters.AddWithValue("@Description", $"Payment for lesson #{lesson.LessonId}");
                        int rowsInserted = insertCmd.ExecuteNonQuery();
                        Console.WriteLine($"[DEBUG] Payment insert affected {rowsInserted} row(s) for LessonId={lesson.LessonId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Error inserting payment for LessonId={lesson.LessonId}: {ex.Message}");
                }
            }

            Console.WriteLine("[DEBUG] UpdateEndedLessons completed.");
        }


        [HttpGet("instructor/{instructorId}")]
        public IActionResult GetPaymentsByInstructor(int instructorId)
        {
            var payments = new List<Payment>();

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                // קריאה לעדכון שיעורים שעברו את זמנם
                UpdateEndedLessons(connection);

                var query = @"
            SELECT 
                p.PaymentId,
                p.StudentId,
                p.InstructorId,
                p.Amount,
                p.PaymentDate,
                p.PaymentMethod,
                p.Description,
                p.Status,
                pr.FirstName,
                pr.LastName
            FROM payments p
            JOIN student s ON s.StudentId = p.StudentId
            JOIN person pr ON pr.PersonId = s.StudentId
            WHERE p.InstructorId = @InstructorId
            ORDER BY p.PaymentDate DESC
        ";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InstructorId", instructorId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            payments.Add(new Payment
                            {
                                PaymentId = reader.GetInt32("PaymentId"),
                                StudentId = reader.GetInt32("StudentId"),
                                InstructorId = reader.GetInt32("InstructorId"),
                                Amount = reader.GetDecimal("Amount"),
                                PaymentDate = reader.GetDateTime("PaymentDate"),
                                PaymentMethod = reader.GetString("PaymentMethod"),
                                Description = reader.GetString("Description"),
                                Status = reader.GetString("Status"),
                                StudentFullName = reader.GetString("FirstName") + " " + reader.GetString("LastName")
                            });
                        }
                    }
                }
            }
            return Ok(payments);
        }







        [HttpGet("student/{studentId}")]
        public IActionResult GetPaymentsByStudent(int studentId)
        {
            var payments = new List<Payment>();

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"
            SELECT 
                PaymentId, 
                StudentId,
                Amount, 
                PaymentDate,
                PaymentMethod,
                Description,
                Status
            FROM payments 
            WHERE StudentId = @StudentId
        ";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        payments.Add(new Payment
                        {
                            PaymentId = reader.GetInt32("PaymentId"),
                            StudentId = reader.GetInt32("StudentId"),
                            Amount = reader.GetDecimal("Amount"),
                            PaymentDate = reader.GetDateTime("PaymentDate"),
                            PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader.GetString("PaymentMethod"),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                            Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString("Status")
                        });
                    }
                }
            }

            return Ok(payments);
        }

        [HttpGet("total/{studentId}")]
        public IActionResult GetTotalPaidByStudent(int studentId)
        {
            decimal totalPaid = 0;

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT COALESCE(SUM(Amount), 0) AS TotalPaid 
                      FROM payments 
                      WHERE StudentId = @StudentId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@StudentId", studentId);

                totalPaid = (decimal)(command.ExecuteScalar() ?? 0);
            }

            return Ok(totalPaid);
        }


        [HttpPost]
        public IActionResult AddPayment([FromBody] Payment payment)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO payments (StudentId, Amount, PaymentDate) 
                              VALUES (@StudentId, @Amount, @PaymentDate)";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@StudentId", payment.StudentId);
                command.Parameters.AddWithValue("@Amount", payment.Amount);
                command.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate);

                command.ExecuteNonQuery();
            }

            return Ok("Payment added successfully");
        }

        [HttpGet("pending")]
        public IActionResult GetPendingPayments()
        {
            var pendingPayments = new List<Payment>();

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT PaymentId, StudentId, Amount, PaymentDate 
                              FROM payments 
                              WHERE Status = 'Pending'";
                var command = new MySqlCommand(query, connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pendingPayments.Add(new Payment
                        {
                            PaymentId = reader.GetInt32("PaymentId"),
                            StudentId = reader.GetInt32("StudentId"),
                            Amount = reader.GetDecimal("Amount"),
                            PaymentDate = reader.GetDateTime("PaymentDate")
                        });
                    }
                }
            }

            return Ok(pendingPayments);
        }

        [HttpGet("report")]
        public IActionResult GeneratePaymentReport()
        {
            // לדוגמה, ניתן להוסיף שאילתה לסטטיסטיקות תשלומים לפי תקופה, תלמיד, או מדריך
            var report = new
            {
                TotalPayments = 0, // מספר כללי של תשלומים
                TotalAmount = 0.0m // סכום כולל
            };

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT COUNT(*) AS TotalPayments, SUM(Amount) AS TotalAmount 
                              FROM payments";
                var command = new MySqlCommand(query, connection);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        report = new
                        {
                            TotalPayments = reader.GetInt32("TotalPayments"),
                            TotalAmount = reader.GetDecimal("TotalAmount")
                        };
                    }
                }
            }

            return Ok(report);
        }
        [HttpDelete("{paymentId}")]
        public IActionResult CancelPayment(int paymentId)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                // לדוגמה: נניח שאתה שומר 'Status' בטבלת payments, נהפוך אותו ל-Canceled
                // או שנמחק פיזית מהטבלה, בהתאם לדרישותיך.
                var query = @"DELETE FROM payments WHERE PaymentId = @PaymentId";
                // לחלופין: 
                // var query = @"UPDATE payments SET Status='Canceled' WHERE PaymentId=@PaymentId";

                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@PaymentId", paymentId);
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                    return Ok("Payment canceled successfully");
                else
                    return NotFound("Payment not found or already canceled");
            }
        }
        [HttpPut("{paymentId}/markPaid")]
        public IActionResult MarkPaymentAsPaid(int paymentId)
        {
            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = "UPDATE payments SET Status='Paid', PaymentMethod='CreditCard' WHERE PaymentId=@PaymentId";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@PaymentId", paymentId);
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                    return Ok("Payment marked as paid");
                else
                    return NotFound("Payment not found");
            }
        }
        [HttpPut("{paymentId}/updatePaymentInfo")]
        public IActionResult UpdatePaymentInfo(int paymentId, [FromBody] Payment payment)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Updating PaymentId {paymentId}: PaymentMethod='{payment.PaymentMethod}', Description='{payment.Description}'");

                using (var connection = _databaseService.GetConnection())
                {
                    connection.Open();
                    var query = "UPDATE payments SET PaymentMethod=@PaymentMethod, Description=@Description WHERE PaymentId=@PaymentId";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Description", payment.Description ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@PaymentId", paymentId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"[DEBUG] PaymentId {paymentId} updated successfully.");
                            return Ok("Payment info updated successfully");
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] PaymentId {paymentId} not found.");
                            return NotFound("Payment not found");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DEBUG] Error updating payment info: " + ex.Message);
                return StatusCode(500, new { message = "Error updating payment info", details = ex.Message });
            }
        }



    }
}
