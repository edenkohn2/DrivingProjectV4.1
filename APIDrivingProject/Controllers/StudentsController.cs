using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using APIDrivingProject.Services;
using APIDrivingProject.Models;
using System.Collections.Generic;

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
       
        public IActionResult Register([FromBody] RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Registration logic
                return Ok(new { message = "User registered successfully", success = true });
            }
            else
            {
                return BadRequest(new { message = "Registration failed", success = false });
            }
        }



        [HttpGet]
        public async Task<List<Student>> GetStudents()
        {
            return await _databaseService.GetAllStudents();
        }



    }
}
