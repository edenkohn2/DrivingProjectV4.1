namespace DrivingClassLibary.Models

{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int StudentId { get; set; }
        public int InstructorId { get; set; }  // <-- הוספת שדה זה
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string StudentFullName { get; set; }
    }





}
