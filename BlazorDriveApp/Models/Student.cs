using System.ComponentModel.DataAnnotations;

namespace BlazorDriveApp.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        public string PhoneNumber { get; set; }

        public int LessonsTaken { get; set; }
        public bool HasPassedTheory { get; set; }
    }

}
