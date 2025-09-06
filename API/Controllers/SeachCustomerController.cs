using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeachCustomerController : ControllerBase
    {
        private readonly ISeachCustomerService _productCustomerService;

        public SeachCustomerController(ISeachCustomerService productCustomerService)
        {
            _productCustomerService = productCustomerService;
        }

        // Action này chỉ phục vụ việc tìm kiếm của khách hàng
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string? keyword)
        {
            var result = await _productCustomerService.SearchProductsAsync(keyword);
            return Ok(result);
        }
    }
}
