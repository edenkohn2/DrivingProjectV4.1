namespace DrivingClassLibary.Models

{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public string LicensePlate { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string LicenseType { get; set; } // Enum for 'Manual', 'Automatic'
        public int InstructorId { get; set; } // Foreign Key to Instructor
    }
    

}
