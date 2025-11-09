using System.ComponentModel.DataAnnotations;

namespace Payment.Models.Payments
{
    public class PaymentResult
    {
        // primary key for sqlite
        [Key]
        public int Id { get; set; }
        public bool Success { get; set; }
        public string? TransactionId { get; set; }
        public string? Message { get; set; }
        public string? ClientSecret { get; set; } // p/ stripe
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        // after webhook
        public string? Status { get; set; }
        public DateTime? RefundedAt { get; set; }

    }
}