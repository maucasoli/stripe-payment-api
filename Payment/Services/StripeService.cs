using Payment.Contracts;
using Payment.Data;
using Payment.Models.Payments;
using Stripe;

namespace Payment.Services
{
    public class StripeService : IPaymentGateway
    {
        private readonly ILogger<StripeService> _logger;
        // read variables from appsettings or .env
        private readonly IConfiguration _configuration;
        public string GatewayName => "Stripe";
        private readonly PaymentsDbContext _db;

        public StripeService(ILogger<StripeService> logger, IConfiguration configuration, PaymentsDbContext db)
        {
            _logger = logger;
            _configuration = configuration;
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
            _db = db;
        }

        public async Task<PaymentResult> ProcessAsync(decimal amount, string currency, string description)
        {
            PaymentResult result;

            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100),
                    Currency = currency,
                    Description = description,
                    PaymentMethodTypes = new List<string> { "card" },
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100),
                    Currency = currency,
                    Description = description,
                    PaymentMethodTypes = new List<string> { "card" }
                });


                result = new PaymentResult
                {
                    Success = true,
                    TransactionId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    Amount = amount,
                    Currency = currency,
                    Message = "Payment processed successfully",
                    ProcessedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe payment failed.");
                result = new PaymentResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }

            // Save to SQLite ==> moved to webhook
            //_db.PaymentResult.Add(result);
            //await _db.SaveChangesAsync();

            return result;
        }
    }
}



//{
//    "cardNumber": "4242424242424242",
//  "expiryDate": "12/26",
//  "amount": 10,
//  "currency": "usd",
//  "description": "Payment test"
//}
