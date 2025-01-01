namespace DrivingClassLibary.Models

{
    public class Lesson
    {
        public int LessonId { get; set; }
        public int StudentId { get; set; }
        public int InstructorId { get; set; }
        public DateTime Date { get; set; }
        public int Duration { get; set; } // 40, 60, or 80 minutes
        public string LessonType { get; set; } // "Single", "OneAndAHalf", or "Double"
        public decimal Price { get; set; }
    }

}
