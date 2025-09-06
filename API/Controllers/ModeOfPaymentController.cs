using API.Domain.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ModeOfPaymentController : ControllerBase
    {
        private readonly IModeOfPaymentService _modeOfPaymentService;

        public ModeOfPaymentController(IModeOfPaymentService modeOfPaymentService)
        {
            _modeOfPaymentService = modeOfPaymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _modeOfPaymentService.GetAllAsync();
            return Ok(result);
        }
    }
}
