using API.Domain.DTOs;
using API.Domain.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PaymentMethodController : ControllerBase
    {
        private readonly IPaymentMethodService _paymentMethodService;

        public PaymentMethodController(IPaymentMethodService paymentMethodService)
        {
            _paymentMethodService = paymentMethodService;
        }

        // GET: api/paymentmethod
        [HttpGet]
        public async Task<ActionResult<List<PaymentMethodDto>>> GetAll()
        {
            var result = await _paymentMethodService.GetAllAsync();
            return Ok(result);
        }
    }
}
