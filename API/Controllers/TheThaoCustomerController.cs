using API.DomainCusTomer.DTOs;
using API.DomainCusTomer.Request.TheThao;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TheThaoCustomerController : ControllerBase
    {
        private readonly ITheThaoCustomerServices _service;

        public TheThaoCustomerController(ITheThaoCustomerServices service)
        {
            _service = service;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDetailCustomerDto>> GetById(Guid id)
        {
            var detail = await _service.GetId(id);
            if (detail == null)
                return NotFound(new { Message = "Chi tiết sản phẩm không tồn tại." });

            return Ok(detail);
        }
        [HttpGet("TheThao")]
        public async Task<IActionResult> TheThao([FromQuery] ProductFilterRequest filter)
        {
            var result = await _service.TheThao(filter);
            return Ok(result);
        }
        [HttpGet("Pickleball")]
        public async Task<IActionResult> Pickleball([FromQuery] PickleballFilterRequest filter)
        {
            var result = await _service.GetALLPICKLEBALL(filter);
            return Ok(result);
        }
        [HttpGet("BongRo")]
        public async Task<IActionResult> BongRo([FromQuery] BongRoFilterRequest filter)
        {
            var result = await _service.GetAllBongRo(filter);
            return Ok(result);
        }
        [HttpGet("BongDa")]
        public async Task<IActionResult> BongDa([FromQuery] BongDaFilterRequest filter)
        {
            var result = await _service.GetAllBongDa(filter);
            return Ok(result);
        }
        [HttpGet("ChayBo")]
        public async Task<IActionResult> ChayBo([FromQuery] ChayBoFilterRequest filter)
        {
            var result = await _service.GetAllChayBo(filter);
            return Ok(result);
        }
        [HttpGet("TapLuyen")]
        public async Task<IActionResult> TapLuyen([FromQuery] TapLuyenFilterRequest filter)
        {
            var result = await _service.GetAllTapLuyen(filter);
            return Ok(result);
        }
        [HttpGet("CauLong")]
        public async Task<IActionResult> CauLong([FromQuery] CauLongFilterRequest filter)
        {
            var result = await _service.GetAllCauLong(filter);
            return Ok(result);
        }
        [HttpGet("Golf")]
        public async Task<IActionResult> Golf([FromQuery] Golf filter)
        {
            var result = await _service.GetAllGolf(filter);
            return Ok(result);
        }




    }
}
