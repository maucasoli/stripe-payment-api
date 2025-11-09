using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payment.Data;
using Payment.Models.Payments;
using Stripe;

[Route("api/[controller]")]
[ApiController]
public class StripeWebhookController : ControllerBase
{
    private readonly string webhookSecret;
    private readonly PaymentsDbContext _db;

    public StripeWebhookController(IConfiguration config, PaymentsDbContext db)
    {
        webhookSecret = config["Stripe:WebhookSecret"];
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeSignature = Request.Headers["Stripe-Signature"];

            // signature validation
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                webhookSecret
            );

            var eventType = stripeEvent.Type;

            switch (eventType)
            {
                case "payment_intent.succeeded":
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                    Console.WriteLine(
                        $"Payment successful: {paymentIntent.Id} - {paymentIntent.AmountReceived / 100.0} {paymentIntent.Currency}"
                    );

                    var result = new PaymentResult
                    {
                        TransactionId = paymentIntent.Id,
                        Amount = paymentIntent.AmountReceived,
                        Currency = paymentIntent.Currency,
                        Status = paymentIntent.Status,
                        ProcessedAt = paymentIntent.Created,
                        Success = paymentIntent.Status == "succeeded",
                        Message = paymentIntent.LastPaymentError?.Message,
                        ClientSecret = paymentIntent.ClientSecret
                    };

                    _db.PaymentResult.Add(result);
                    await _db.SaveChangesAsync();
                    break;

                case "payment_intent.payment_failed":
                    var failedIntent = stripeEvent.Data.Object as PaymentIntent;

                    result = new PaymentResult
                    {
                        TransactionId = failedIntent.Id,
                        Amount = failedIntent.Amount,
                        Currency = failedIntent.Currency,
                        Status = failedIntent.Status,
                        Message = failedIntent.LastPaymentError?.Message,
                        Success = false,
                        ProcessedAt = failedIntent.Created
                    };

                    _db.PaymentResult.Add(result);
                    await _db.SaveChangesAsync();

                    Console.WriteLine($"Payment failed: {failedIntent.Id} - {failedIntent.LastPaymentError?.Message}");
                    break;

                case "charge.refunded":
                    var charge = stripeEvent.Data.Object as Charge;
                    Console.WriteLine(
                        $"Refund: {charge.Id} - {charge.AmountRefunded / 100.0} {charge.Currency}"
                    );

                    var paymentIntentId = charge.PaymentIntentId;

                    if (!string.IsNullOrEmpty(paymentIntentId))
                    {
                        var existing = await _db.PaymentResult
                            .FirstOrDefaultAsync(p => p.TransactionId == paymentIntentId);

                        if (existing != null)
                        {
                            existing.Status = "refunded";

                            // se houver refund no array, pega a primeira, senão usa Created do charge
                            if (charge.Refunds?.Data?.Count > 0)
                            {
                                existing.RefundedAt = charge.Refunds.Data[0].Created;
                            }
                            else
                            {
                                existing.RefundedAt = charge.Created;
                            }

                            await _db.SaveChangesAsync();
                        }
                    }
                    break;

                default:
                    Console.WriteLine($"Event received: {eventType}");
                    break;
            }

            return Ok();
        }
        catch (StripeException e)
        {
            Console.WriteLine($"Stripe error: {e.Message}");
            return BadRequest();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return BadRequest();
        }
    }
}
