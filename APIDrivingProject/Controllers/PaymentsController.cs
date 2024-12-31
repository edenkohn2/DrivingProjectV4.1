using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using APIDrivingProject.Services;
using APIDrivingProject.Models;
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

        [HttpGet]
        public IActionResult GetPayments()
        {
            var payments = new List<Payment>();

            using (var connection = _databaseService.GetConnection())
            {
                connection.Open();
                var query = @"SELECT PaymentId, StudentId, Amount, PaymentDate 
                              FROM payments";
                var command = new MySqlCommand(query, connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        payments.Add(new Payment
                        {
                            PaymentId = reader.GetInt32("PaymentId"),
                            StudentId = reader.GetInt32("StudentId"),
                            Amount = reader.GetDecimal("Amount"),
                            PaymentDate = reader.GetDateTime("PaymentDate")
                        });
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
                var query = @"SELECT PaymentId, Amount, PaymentDate 
                              FROM payments 
                              WHERE StudentId = @StudentId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        payments.Add(new Payment
                        {
                            PaymentId = reader.GetInt32("PaymentId"),
                            Amount = reader.GetDecimal("Amount"),
                            PaymentDate = reader.GetDateTime("PaymentDate")
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
    }
}
