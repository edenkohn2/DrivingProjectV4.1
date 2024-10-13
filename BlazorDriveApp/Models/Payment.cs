namespace BlazorDriveApp.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int StudentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentType { get; set; }
    }
}
