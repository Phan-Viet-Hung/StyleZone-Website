using API.DomainCusTomer.DTOs.DetailCustomer;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetailCustomerController : ControllerBase
    {
        private readonly IDetailCustomerServices _service;

        public DetailCustomerController(IDetailCustomerServices service)
        {
            _service = service;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<DetailCustomerDto>> GetById(Guid id)
        {
            var detail = await _service.GetId(id);
            if (detail == null)
                return NotFound(new { Message = "Chi tiết sản phẩm không tồn tại." });

            return Ok(detail);
        }

        // POST api/<DetailCustomerController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<DetailCustomerController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<DetailCustomerController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
