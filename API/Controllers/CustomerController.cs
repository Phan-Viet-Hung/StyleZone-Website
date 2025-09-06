using API.Domain.DTOs;
using API.Domain.Request.CustomerRequest;
using API.Domain.Service.IService.ICustomerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetById(Guid id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] string status)
        {
            var updated = await _customerService.UpdateStatusAsync(id, status);
            if (!updated) return NotFound();
            return NoContent();
        }
        [HttpPost("status/bulk")]
        public async Task<IActionResult> UpdateStatusBulk([FromBody] UpdateStatusBulkRequest request)
        {
            if (request == null || request.Ids == null || !request.Ids.Any() || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            var updatedCount = await _customerService.UpdateStatusBulkAsync(request.Ids, request.Status);
            return Ok(new { Updated = updatedCount });
        }

    }
}
