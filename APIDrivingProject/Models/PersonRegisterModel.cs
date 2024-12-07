namespace APIDrivingProject.Models
{
    public class PersonRegisterModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // טקסט רגיל
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Role { get; set; }
    }


}
