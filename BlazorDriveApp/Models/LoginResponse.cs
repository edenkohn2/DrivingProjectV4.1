namespace BlazorDriveApp.Models

{
    using DrivingProjectSharedModels.Models;

    public class LoginResponse
    {
        public string Message { get; set; }
        public User User { get; set; }
    }
}
