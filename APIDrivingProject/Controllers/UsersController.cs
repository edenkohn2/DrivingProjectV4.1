using Microsoft.AspNetCore.Mvc;
using APIDrivingProject.Models;
using DrivingProjectSharedModels.Models;
using APIDrivingProject.Services;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace APIDrivingProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // רישום משתמש חדש
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] PersonLoginModel model)
        {
            var person = await _userService.Login(model.Email, model.Password);
            if (person == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(new
            {
                message = "Login successful",
                person = new
                {
                    person.PersonId,
                    person.FirstName,
                    person.LastName,
                    person.Email,
                    person.Role // החזרת Role בתשובה
                }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] PersonRegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var person = new Person
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = model.Password,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                BirthDate = model.BirthDate,
                Role = model.Role
            };

            bool isRegistered = await _userService.RegisterPerson(person);

            if (isRegistered)
                return Ok("User registered successfully");

            return BadRequest("Registration failed");
        }

        [HttpGet("check-email")]
        public async Task<IActionResult> IsEmailUnique([FromQuery] string email)
        {
            using var connection = _userService.GetDatabaseConnection();
            await connection.OpenAsync();

            var query = "SELECT COUNT(*) FROM person WHERE Email = @Email";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return Ok(count == 0); // מחזיר true אם האימייל פנוי
        }

    }
}
