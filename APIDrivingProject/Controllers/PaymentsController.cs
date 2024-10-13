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

        // GET: api/Payments
        [HttpGet]
        public IActionResult GetPayments()
        {
            var payments = new List<Payment>();

            using (MySqlConnection connection = _databaseService.GetConnection())
            {
                connection.Open();
                string query = "SELECT PaymentId, StudentId, Amount, PaymentDate, PaymentType FROM Payments";
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var payment = new Payment
                    {
                        PaymentId = reader.GetInt32(0),
                        StudentId = reader.GetInt32(1),
                        Amount = reader.GetDecimal(2),
                        PaymentDate = reader.GetDateTime(3),
                        PaymentType = reader.GetString(4)
                    };
                    payments.Add(payment);
                }

                reader.Close();
            }

            return Ok(payments);
        }
    }
}
