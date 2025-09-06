using API.DomainCusTomer.Request.GHN;
using API.DomainCusTomer.Services;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class GHNController : ControllerBase
    {
        private readonly IGhnService _ghnService;

        public GHNController(IGhnService ghnService)
        {
            _ghnService = ghnService;
        }


        [HttpPost("calculate-fee")]
        public async Task<IActionResult> CalculateFee([FromBody] ShippingFeeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Dữ liệu không hợp lệ." });

            try
            {
                // Lấy service_id hợp lệ từ GHN
                int serviceId = await _ghnService.GetAvailableServicesAsync(request.to_district_id);
                request.service_id = serviceId;

                decimal fee = await _ghnService.CalculateFeeAsync(request);
                return Ok(new { total_fee = fee });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var json = await _ghnService.GetProvincesAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);
            return Ok(data.GetProperty("data"));
        }

        [HttpPost("districts")]
        public async Task<IActionResult> GetDistricts([FromBody] JsonElement body)
        {
            int provinceId = body.GetProperty("province_id").GetInt32();
            var json = await _ghnService.GetDistrictsAsync(provinceId);
            var data = JsonSerializer.Deserialize<JsonElement>(json);
            return Ok(data.GetProperty("data"));
        }

        [HttpPost("wards")]
        public async Task<IActionResult> GetWards([FromBody] JsonElement body)
        {
            int districtId = body.GetProperty("district_id").GetInt32();
            var json = await _ghnService.GetWardsAsync(districtId);
            var data = JsonSerializer.Deserialize<JsonElement>(json);
            return Ok(data.GetProperty("data"));
        }

    }
}
