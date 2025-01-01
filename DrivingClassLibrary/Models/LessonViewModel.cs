namespace DrivingClassLibary.Models

{
    public class LessonViewModel
    {
        public int LessonId { get; set; }
        public DateTime Date { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string LessonType { get; set; } = string.Empty;
        public int Duration { get; set; }
        public decimal Price { get; set; }
    }
}
