using API.DomainCusTomer.Request.Nu;
using API.DomainCusTomer.Request.TheThao;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NuCustomerController : ControllerBase
    {
        private readonly INuCustomer _service;

        public NuCustomerController(INuCustomer service)
        {
            _service = service;
        }
        [HttpGet("DoNu")]
        public async Task<IActionResult> GetDoNuCustomer([FromQuery] DoNuCustomerFilterRequest filter)
        {
            var result = await _service.DoNuCustomer(filter);
            return Ok(result);
        }

        [HttpGet("AoNu")]
        public async Task<IActionResult> GetAoNu([FromQuery] AoNuFilterRequest filter)
        {
            var result = await _service.AoNu(filter);
            return Ok(result);
        }

        [HttpGet("AoTShirtNu")]
        public async Task<IActionResult> GetAoTShirtNu([FromQuery] AoTShirtNuFilterRequest filter)
        {
            var result = await _service.AoTShirtNu(filter);
            return Ok(result);
        }

        [HttpGet("AoPoLoNu")]
        public async Task<IActionResult> GetAoPoLoNu([FromQuery] AoPoLoNuFilterRequest filter)
        {
            var result = await _service.AoPoLoNu(filter);
            return Ok(result);
        }

        [HttpGet("AoGioNu")]
        public async Task<IActionResult> GetAoGioNu([FromQuery] AoGioNuFilterRequest filter)
        {
            var result = await _service.AoGioNu(filter);
            return Ok(result);
        }

        [HttpGet("AoNiNu")]
        public async Task<IActionResult> GetAoNiNu([FromQuery] AoNiNuFilterRequest filter)
        {
            var result = await _service.AoNiNu(filter);
            return Ok(result);
        }

        [HttpGet("AoDaiTayNu")]
        public async Task<IActionResult> GetAoDaiTayNu([FromQuery] AoDaiTayNuFilterRequest filter)
        {
            var result = await _service.AoDaiTayNu(filter);
            return Ok(result);
        }

        [HttpGet("AoLongVuNu")]
        public async Task<IActionResult> GetAoLongVuNu([FromQuery] AoLongVuNuFilterRequest filter)
        {
            var result = await _service.AoLongVuNu(filter);
            return Ok(result);
        }

        [HttpGet("QuanNu")]
        public async Task<IActionResult> GetQuanNu([FromQuery] QuanNuFilterRequest filter)
        {
            var result = await _service.QuanNu(filter);
            return Ok(result);
        }

        [HttpGet("QuanShortNu")]
        public async Task<IActionResult> GetQuanShortNu([FromQuery] QuanShortNuFilterRequest filter)
        {
            var result = await _service.QuanShortNu(filter);
            return Ok(result);
        }

        [HttpGet("QuanGioNu")]
        public async Task<IActionResult> GetQuanGioNu([FromQuery] QuanGioNuFilterRequest filter)
        {
            var result = await _service.QuanGioNu(filter);
            return Ok(result);
        }

        [HttpGet("QuanNiNu")]
        public async Task<IActionResult> GetQuanNiNu([FromQuery] QuanNiNuFilterRequest filter)
        {
            var result = await _service.QuanNiNu(filter);
            return Ok(result);
        }

        [HttpGet("GiayNu")]
        public async Task<IActionResult> GetGiayNu([FromQuery] GiayNuFilterRequest filter)
        {
            var result = await _service.GiayNu(filter);
            return Ok(result);
        }

        [HttpGet("GiayThoiTrangNu")]
        public async Task<IActionResult> GetGiayThoiTrangNu([FromQuery] GiayThoiTrangNuFilterRequest filter)
        {
            var result = await _service.GiayThoiTrangNu(filter);
            return Ok(result);
        }

        [HttpGet("GiayChayBoNu")]
        public async Task<IActionResult> GetGiayChayBoNu([FromQuery] GiayChayBoNuFilterRequest filter)
        {
            var result = await _service.GiayChayBoNu(filter);
            return Ok(result);
        }

        [HttpGet("GiayCauLongNu")]
        public async Task<IActionResult> GetGiayCauLongNu([FromQuery] GiayCauLongNuFilterRequest filter)
        {
            var result = await _service.GiayCauLongNu(filter);
            return Ok(result);
        }

        [HttpGet("GiayBongRoNu")]
        public async Task<IActionResult> GetGiayBongRoNu([FromQuery] GiayBongRoNuFilterRequest filter)
        {
            var result = await _service.GiayBongRoNu(filter);
            return Ok(result);
        }

        [HttpGet("GiayBongDaNu")]
        public async Task<IActionResult> GetGiayBongDaNu([FromQuery] GiayBongDaNuFilterRequest filter)
        {
            var result = await _service.GiayBongDaNu(filter);
            return Ok(result);
        }

        [HttpGet("BoQuanAoNu")]
        public async Task<IActionResult> GetBoQuanAoNu([FromQuery] BoQuanAoNuFilterRequest filter)
        {
            var result = await _service.BoQuanAoNu(filter);
            return Ok(result);
        }

        [HttpGet("BoQuanAoBongRoNu")]
        public async Task<IActionResult> GetBoQuanAoBongRoNu([FromQuery] BoQuanAoBongRoNuFilterRequest filter)
        {
            var result = await _service.BoQuanAoBongRoNu(filter);
            return Ok(result);
        }

        [HttpGet("BoQuanAoCauLongNu")]
        public async Task<IActionResult> GetBoQuanAoCauLongNu([FromQuery] BoQuanAoCauLongNuFilterRequest filter)
        {
            var result = await _service.BoQuanAoCauLongNu(filter);
            return Ok(result);
        }
    }
}
