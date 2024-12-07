using System.ComponentModel.DataAnnotations;

namespace BlazorDriveApp.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int LessonsTaken { get; set; }
        public bool HasPassedTheory { get; set; }
    }


}
