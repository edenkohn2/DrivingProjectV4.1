namespace APIDrivingProject.Models
{
    public class Person
    {
        public int PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Role { get; set; } // "Student", "Instructor", or "Admin"
    }

}
