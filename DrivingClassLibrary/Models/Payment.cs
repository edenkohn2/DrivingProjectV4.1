namespace DrivingClassLibary.Models

{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int StudentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }

        // שדות חסרים שהוספתי
        public string PaymentMethod { get; set; } // לדוגמה: "Credit Card", "Cash"
        public string Description { get; set; } // לדוגמה: "Lesson Payment"
    }


}
