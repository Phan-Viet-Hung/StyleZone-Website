using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TinTucCustomerController : ControllerBase
    {
        private readonly ITinTucService _tinTucService;
        public TinTucCustomerController(ITinTucService tinTucService)
        {
            _tinTucService = tinTucService;
        }

        [HttpGet] // Route: GET /api/TinTuc
        public async Task<IActionResult> GetAll()
        {
            var result = await _tinTucService.GetAllTinTucAsync();
            return Ok(result);
        }

        [HttpGet("{id}")] // Route: GET /api/TinTuc/{id}
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _tinTucService.GetTinTucByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
