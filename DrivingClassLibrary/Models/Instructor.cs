namespace DrivingClassLibary.Models

{
    public class Instructor : Person
    {
        public int InstructorId => PersonId;
        public int ExperienceYears { get; set; } = 0;
    }

}
