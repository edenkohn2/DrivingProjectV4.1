namespace BlazorDriveApp.Models

{
    using DrivingClassLibary.Models;

    public class LoginResponse
    {
        public string Message { get; set; }
        public User User { get; set; }
    }
}
