using API.Domain.Request.OriginRequest;
using API.Domain.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class OriginController : ControllerBase
    {
        private readonly IOriginService _service;

        public OriginController(IOriginService service)
        {
            _service = service;
        }

        private IActionResult ProcessModelStateErrors()
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage).ToList();

            var errorsVi = errors.Select(err =>
            {
                if ((err.Contains("is not valid") || err.Contains("The value") && err.Contains("is invalid")))
                    return "Giá trị nhập không hợp lệ, vui lòng kiểm tra lại kiểu dữ liệu.";
                if (err.Contains("must be a number"))
                    return "Trường này phải là số hợp lệ.";
                if (err.Contains("The field") && err.Contains("must be a valid date"))
                    return "Trường này phải là ngày hợp lệ.";
                return err;
            }).ToList();

            return BadRequest(new { Errors = errorsVi });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "ID không hợp lệ." });

            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = "Không tìm thấy xuất xứ." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOriginRequest request)
        {
            if (!ModelState.IsValid)
                return ProcessModelStateErrors();

            // Validate các trường cụ thể
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Tên xuất xứ không được để trống." });

            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 500)
                return BadRequest(new { message = "Mô tả không được vượt quá 500 ký tự." });

            try
            {
                var result = await _service.CreateAsync(request);
                return Ok(new { message = "Tạo xuất xứ thành công", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateOriginRequest request)
        {
            if (!ModelState.IsValid)
                return ProcessModelStateErrors();

            if (request.Id == Guid.Empty)
                return BadRequest(new { message = "ID không hợp lệ." });

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Tên xuất xứ không được để trống." });

            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 500)
                return BadRequest(new { message = "Mô tả không được vượt quá 500 ký tự." });

            try
            {
                var result = await _service.UpdateAsync(request);
                return Ok(new { message = "Cập nhật xuất xứ thành công", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
