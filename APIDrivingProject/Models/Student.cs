using System.ComponentModel.DataAnnotations;

namespace APIDrivingProject.Models
{
    public class Student : Person
    {
        public int LessonsTaken { get; set; } = 0;
        public bool HasPassedTheory { get; set; } = false;
        public bool IsActive { get; set; } = true; // To indicate if the student is still learning
    }


}
