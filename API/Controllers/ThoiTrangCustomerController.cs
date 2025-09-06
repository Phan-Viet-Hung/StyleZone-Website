using API.DomainCusTomer.Request;
using API.DomainCusTomer.Request.ThoiTrang;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThoiTrangCustomerController : ControllerBase
    {
        private readonly IThoiTrangCustomerServices _service;

        public ThoiTrangCustomerController(IThoiTrangCustomerServices service)
        {
            _service = service;
        }

        [HttpGet("OUTLET")]
        public async Task<IActionResult> OUTLET([FromQuery] OUTLETFilterRequst filter)
        {
            var result = await _service.GetAllOUTLET(filter);
            return Ok(result);
        }
        [HttpGet("OUTLETPICKLEBALL")]
        public async Task<IActionResult> OUTLETPICKLEBALL([FromQuery] OUTLETPICKLEBALLFilterRequst filter)
        {
            var result = await _service.GetAllOUTLETPICKLEBALL(filter);
            return Ok(result);
        }
        [HttpGet("YOUNG")]
        public async Task<IActionResult> YOUNG([FromQuery] YOUNGFilterRequst filter)
        {
            var result = await _service.GetAllYOUNG(filter);
            return Ok(result);
        }
        [HttpGet("Betrai")]
        public async Task<IActionResult> Betrai([FromQuery] BeTraiFilterRequst filter)
        {
            var result = await _service.GetAllBeTrai(filter);
            return Ok(result);
        }
        [HttpGet("BeGai")]
        public async Task<IActionResult> BeGai([FromQuery] BeGaiFilterRequst filter)
        {
            var result = await _service.GetAllBeGai(filter);
            return Ok(result);
        }
        [HttpGet("ISAAC")]
        public async Task<IActionResult> ISAAC([FromQuery] ISAACFilterRequst filter)
        {
            var result = await _service.GetAllISAAC(filter);
            return Ok(result);
        }
        [HttpGet("LIFESTYLE")]
        public async Task<IActionResult> LIFESTYLE([FromQuery] LIFESTYLEFilterRequst filter)
        {
            var result = await _service.GetAllLIFESTYLE(filter);
            return Ok(result);
        }
        [HttpGet("BADFIVE")]
        public async Task<IActionResult> BADFIVE([FromQuery] BADFIVEFilterRequst filter)
        {
            var result = await _service.GetAllBADFIVE(filter);
            return Ok(result);
        }

        [HttpGet("WADE")]
        public async Task<IActionResult> WADE([FromQuery] WadeFilterRequst filter)
        {
            var result = await _service.GetAllWaDe(filter);
            return Ok(result);
        }
        [HttpGet("ThoiTrang")]
        public async Task<IActionResult> ThoiTrang([FromQuery] ThoiTrangFilterRequst filter)
        {
            var result = await _service.GetALLThoiTrang(filter);
            return Ok(result);
        }


    }
}
