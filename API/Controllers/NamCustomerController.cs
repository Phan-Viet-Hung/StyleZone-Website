using API.DomainCusTomer.Request.Nam;
using API.DomainCusTomer.Request.TheThao;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NamCustomerController : ControllerBase
    {
        private readonly INamCustomer _service;

        public NamCustomerController(INamCustomer service)
        {
            _service = service;
        }
        [HttpGet("DoNam")]
        public async Task<IActionResult> GetDoNamCustomer([FromQuery] DoNamCustomerFilterRequest filter)
        {
            var result = await _service.DoNamCustomer(filter);
            return Ok(result);
        }

        [HttpGet("AoNam")]
        public async Task<IActionResult> GetAoNam([FromQuery] AoNamFilterRequest filter)
        {
            var result = await _service.AoNam(filter);
            return Ok(result);
        }

        [HttpGet("AoTShirtNam")]
        public async Task<IActionResult> GetAoTShirtNam([FromQuery] AoTShirtNamFilterRequest filter)
        {
            var result = await _service.AoTShirtNam(filter);
            return Ok(result);
        }

        [HttpGet("AoPoLoNam")]
        public async Task<IActionResult> GetAoPoLoNam([FromQuery] AoPoLoNamFilterRequest filter)
        {
            var result = await _service.AoPoLoNam(filter);
            return Ok(result);
        }

        [HttpGet("AoGioNam")]
        public async Task<IActionResult> GetAoGioNam([FromQuery] AoGioNamFilterRequest filter)
        {
            var result = await _service.AoGioNam(filter);
            return Ok(result);
        }

        [HttpGet("AoNiNam")]
        public async Task<IActionResult> GetAoNiNam([FromQuery] AoNiNamFilterRequest filter)
        {
            var result = await _service.AoNiNam(filter);
            return Ok(result);
        }

        [HttpGet("AoDaiTayNam")]
        public async Task<IActionResult> GetAoDaiTayNam([FromQuery] AoDaiTayNamFilterRequest filter)
        {
            var result = await _service.AoDaiTayNam(filter);
            return Ok(result);
        }

        [HttpGet("AoLongVuNam")]
        public async Task<IActionResult> GetAoLongVuNam([FromQuery] AoLongVuNamFilterRequest filter)
        {
            var result = await _service.AoLongVuNam(filter);
            return Ok(result);
        }

        [HttpGet("QuanNam")]
        public async Task<IActionResult> GetQuanNam([FromQuery] QuanNamFilterRequest filter)
        {
            var result = await _service.QuanNam(filter);
            return Ok(result);
        }

        [HttpGet("QuanShortNam")]
        public async Task<IActionResult> GetQuanShortNam([FromQuery] QuanShortNamFilterRequest filter)
        {
            var result = await _service.QuanShortNam(filter);
            return Ok(result);
        }

        [HttpGet("QuanGioNam")]
        public async Task<IActionResult> GetQuanGioNam([FromQuery] QuanGioNamFilterRequest filter)
        {
            var result = await _service.QuanGioNam(filter);
            return Ok(result);
        }

        [HttpGet("QuanNiNam")]
        public async Task<IActionResult> GetQuanNiNam([FromQuery] QuanNiNamFilterRequest filter)
        {
            var result = await _service.QuanNiNam(filter);
            return Ok(result);
        }

        [HttpGet("GiayNam")]
        public async Task<IActionResult> GetGiayNam([FromQuery] GiayNamFilterRequest filter)
        {
            var result = await _service.GiayNam(filter);
            return Ok(result);
        }

        [HttpGet("GiayThoiTrangNam")]
        public async Task<IActionResult> GetGiayThoiTrangNam([FromQuery] GiayThoiTrangNamFilterRequest filter)
        {
            var result = await _service.GiayThoiTrangNam(filter);
            return Ok(result);
        }

        [HttpGet("GiayChayBoNam")]
        public async Task<IActionResult> GetGiayChayBoNam([FromQuery] GiayChayBoNamFilterRequest filter)
        {
            var result = await _service.GiayChayBoNam(filter);
            return Ok(result);
        }

        [HttpGet("GiayCauLongNam")]
        public async Task<IActionResult> GetGiayCauLongNam([FromQuery] GiayCauLongNamFilterRequest filter)
        {
            var result = await _service.GiayCauLongNam(filter);
            return Ok(result);
        }

        [HttpGet("GiayBongRoNam")]
        public async Task<IActionResult> GetGiayBongRoNam([FromQuery] GiayBongRoNamFilterRequest filter)
        {
            var result = await _service.GiayBongRoNam(filter);
            return Ok(result);
        }

        [HttpGet("GiayBongDaNam")]
        public async Task<IActionResult> GetGiayBongDaNam([FromQuery] GiayBongDaNamFilterRequest filter)
        {
            var result = await _service.GiayBongDaNam(filter);
            return Ok(result);
        }

        [HttpGet("BoQuanAoNam")]
        public async Task<IActionResult> GetBoQuanAoNam([FromQuery] BoQuanAoNamFilterRequest filter)
        {
            var result = await _service.BoQuanAoNam(filter);
            return Ok(result);
        }

        [HttpGet("BoQuanAoBongRoNam")]
        public async Task<IActionResult> GetBoQuanAoBongRoNam([FromQuery] BoQuanAoBongRoNamFilterRequest filter)
        {
            var result = await _service.BoQuanAoBongRoNam(filter);
            return Ok(result);
        }

        [HttpGet("BoQuanAoCauLongNam")]
        public async Task<IActionResult> GetBoQuanAoCauLongNam([FromQuery] BoQuanAoCauLongNamFilterRequest filter)
        {
            var result = await _service.BoQuanAoCauLongNam(filter);
            return Ok(result);
        }


    }

}
