using Payment.Contracts;
using Payment.Models.Payments;

namespace Payment.Processors
{
    public class PaymentProcessor
    {
        // coleção de objetos que implementam a interface IPaymentGateway (stripe/paypal services)
        private readonly IEnumerable<IPaymentGateway> _gateways;
        private readonly ILogger<PaymentProcessor> _logger;

        public PaymentProcessor(IEnumerable<IPaymentGateway> gateways, ILogger<PaymentProcessor> logger)
        {
            _gateways = gateways;
            _logger = logger;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(string gatewayName, CreditCardPaymentRequest request)
        {
            // escolhe o gateway correto dentro dos gateways
            var gateway = _gateways.FirstOrDefault(g => g.GatewayName.Equals(gatewayName, StringComparison.OrdinalIgnoreCase));
            if (gateway == null)
            {
                return new PaymentResult
                {
                    Success = false,
                    Message = $"Payment gateway '{gatewayName}' not found."
                };
            }

            _logger.LogInformation($"Processing payment via {gatewayName} for {request.Amount} {request.Currency}");
            return await gateway.ProcessAsync(request.Amount, request.Currency, request.Description);
        }
    }
}
