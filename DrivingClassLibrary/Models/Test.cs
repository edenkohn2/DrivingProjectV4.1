namespace DrivingClassLibary.Models

{
    public class Test
    {
        public int TestId { get; set; }
        public int StudentId { get; set; } // Foreign Key to Student
        public string TestType { get; set; } // Enum for 'Internal', 'External'
        public DateTime Date { get; set; }
        public bool Passed { get; set; }
    }

}
