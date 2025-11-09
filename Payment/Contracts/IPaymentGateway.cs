using Payment.Models.Payments;

namespace Payment.Contracts
{
    public interface IPaymentGateway
    {
        Task<PaymentResult> ProcessAsync(decimal amount, string currency, string description);
        string GatewayName { get; }
    }
}