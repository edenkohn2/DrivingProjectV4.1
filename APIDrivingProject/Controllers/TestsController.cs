using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using APIDrivingProject.Services;
using APIDrivingProject.Models;
using System.Collections.Generic;
namespace APIDrivingProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public TestsController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // GET: api/Tests
        [HttpGet]
        public IActionResult GetTests()
        {
            var tests = new List<Test>();

            using (MySqlConnection connection = _databaseService.GetConnection())
            {
                connection.Open();
                string query = "SELECT TestId, StudentId, TestType, Date, Passed FROM Tests";
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var test = new Test
                    {
                        TestId = reader.GetInt32(0),
                        StudentId = reader.GetInt32(1),
                        TestType = reader.GetString(2),
                        Date = reader.GetDateTime(3),
                        Passed = reader.GetBoolean(4)
                    };
                    tests.Add(test);
                }

                reader.Close();
            }

            return Ok(tests);
        }
    }
}
