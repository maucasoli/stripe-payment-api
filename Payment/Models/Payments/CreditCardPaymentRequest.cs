using System.ComponentModel.DataAnnotations;

namespace Payment.Models.Payments
{
    public class CreditCardPaymentRequest
    {
        [Required]
        public string CardNumber { get; set; }

        [Required]
        public string ExpiryDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "cad";

        public string Description { get; set; }
    }
}

//{
//"cardNumber": "4242424242424242",
//  "expiryDate": "12/26",
//  "amount": 10,
//  "currency": "usd",
//  "description": "Payment test"
//}