using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrivingClassLibary.Models
{
    public class InstructorSettingsModel
    {
        [Key]
        public int InstructorId { get; set; } // מזהה המורה

        [Required]
        [Range(20, 120, ErrorMessage = "משך השיעור חייב להיות בין 20 ל-120 דקות")]
        public int SingleLessonDuration { get; set; } = 40; // ברירת מחדל: 40 דקות

        [Required]
        [Range(50, 1000, ErrorMessage = "מחיר חייב להיות בין 50 ל-1000 ₪")]
        public decimal SingleLessonPrice { get; set; } = 100.00m; // ברירת מחדל: 100 ש"ח

        [NotMapped] // שדות מחושבים שלא נשמרים ישירות בבסיס הנתונים
        public int OneAndAHalfLessonDuration => (int)(SingleLessonDuration * 1.5);

        [NotMapped]
        public decimal OneAndAHalfLessonPrice => SingleLessonPrice * 1.5m;

        [NotMapped]
        public int DoubleLessonDuration => SingleLessonDuration * 2;

        [NotMapped]
        public decimal DoubleLessonPrice => SingleLessonPrice * 2m;
    }
}
