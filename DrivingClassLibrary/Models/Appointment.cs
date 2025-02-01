using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrivingClassLibary.Models
{
    public class Appointment
    {
        public int LessonId { get; set; }  // מזהה ייחודי של השיעור

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string StudentName { get; set; }
        public string LessonType { get; set; }
        public decimal Price { get; set; }
    }



}
