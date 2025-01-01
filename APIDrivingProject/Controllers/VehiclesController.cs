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
    public class VehiclesController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public VehiclesController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // GET: api/Vehicles
        [HttpGet]
        public IActionResult GetVehicles()
        {
            var vehicles = new List<Vehicle>();

            using (MySqlConnection connection = _databaseService.GetConnection())
            {
                connection.Open();
                string query = "SELECT VehicleId, LicensePlate, Manufacturer, Model, LicenseType FROM Vehicles";
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var vehicle = new Vehicle
                    {
                        VehicleId = reader.GetInt32(0),
                        LicensePlate = reader.GetString(1),
                        Manufacturer = reader.GetString(2),
                        Model = reader.GetString(3),
                        LicenseType = reader.GetString(4)
                    };
                    vehicles.Add(vehicle);
                }

                reader.Close();
            }

            return Ok(vehicles);
        }
    }
}
