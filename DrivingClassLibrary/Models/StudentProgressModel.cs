using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrivingClassLibary.Models
{
    public class StudentProgressModel
    {
        public string StudentName { get; set; }
        public string Email { get; set; }
        public int LessonsTaken { get; set; }
        public bool HasPassedTheory { get; set; }
    }
}
