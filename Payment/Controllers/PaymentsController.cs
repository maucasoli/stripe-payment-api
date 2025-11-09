using Microsoft.AspNetCore.Mvc;
using Payment.Models.Payments;
using Payment.Processors;

namespace Payment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentProcessor _processor;
        private readonly ILogger<PaymentsController> _logger;

        // ** note to myself **
        // controllers sao criados automaticamente pelo dependency injection
        public PaymentsController(PaymentProcessor processor, ILogger<PaymentsController> logger)
        {
            _processor = processor;
            _logger = logger;
        }

        // PONTO DE ENTRADA: client -> route -> controller -> processor -> service
        [HttpPost("{gatewayName}")]
        public async Task<IActionResult> ProcessPayment(string gatewayName, [FromBody] CreditCardPaymentRequest request)
        {
            // ** note to myself **
            // ModelState é uma estrutura interna do ASP.NET Core MVC
            // que guarda o estado da validação do modelo
            // recebido em uma requisição HTTP

            // POST: json -> action ASP.NET -> convert json to model object (payment request)
            // -> validate [Required] -> result to model state (true or false)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _processor.ProcessPaymentAsync(gatewayName, request);
            // res 200 : res 400
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
