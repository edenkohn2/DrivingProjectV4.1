namespace APIDrivingProject.Models
{
    public class Lesson
    {
        public int LessonId { get; set; }
        public int StudentId { get; set; }
        public int InstructorId { get; set; }
        public DateTime Date { get; set; }
        public int Duration { get; set; }
        public string LessonType { get; set; }
        public decimal Price { get; set; }
    }
}
