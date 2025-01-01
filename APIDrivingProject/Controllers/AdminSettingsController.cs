using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using APIDrivingProject.Services;
using APIDrivingProject.Models;
using DrivingProjectSharedModels.Models;
using System.Collections.Generic;
namespace APIDrivingProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminSettingsController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public AdminSettingsController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // GET: api/AdminSettings
        [HttpGet]
        public IActionResult GetAdminSettings()
        {
            var settings = new List<AdminSetting>();

            using (MySqlConnection connection = _databaseService.GetConnection())
            {
                connection.Open();
                string query = "SELECT SettingId, SettingName, Value FROM AdminSettings";
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var setting = new AdminSetting
                    {
                        SettingId = reader.GetInt32(0),
                        SettingName = reader.GetString(1),
                        Value = reader.GetString(2)
                    };
                    settings.Add(setting);
                }

                reader.Close();
            }

            return Ok(settings);
        }
    }
}
