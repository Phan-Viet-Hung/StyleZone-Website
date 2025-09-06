using API.Domain.Request.MaterialRequest;
using API.Domain.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StyleZone_API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class MaterialController : ControllerBase
    {
        private readonly IMaterialService _materialService;

        public MaterialController(IMaterialService materialService)
        {
            _materialService = materialService;
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

        // GET: api/Material
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var materials = await _materialService.GetAllAsync();
                return Ok(materials);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Material/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "ID không hợp lệ." });

            try
            {
                var material = await _materialService.GetByIdAsync(id);
                if (material == null)
                    return NotFound(new { message = "Không tìm thấy chất liệu." });

                return Ok(material);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/Material
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateMaterialRequest request)
        {
            if (!ModelState.IsValid)
                return ProcessModelStateErrors();

            // Validate các trường
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Tên chất liệu không được để trống." });

            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 500)
                return BadRequest(new { message = "Mô tả không được vượt quá 500 ký tự." });

            try
            {
                var result = await _materialService.CreateAsync(request);
                return Ok(new { message = "Tạo chất liệu thành công", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Material
        [HttpPut]
        public async Task<IActionResult> Update([FromForm] UpdateMaterialRequest request)
        {
            if (!ModelState.IsValid)
                return ProcessModelStateErrors();

            if (request.Id == Guid.Empty)
                return BadRequest(new { message = "ID không hợp lệ." });

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Tên chất liệu không được để trống." });

            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 500)
                return BadRequest(new { message = "Mô tả không được vượt quá 500 ký tự." });

            try
            {
                var result = await _materialService.UpdateAsync(request);
                return Ok(new { message = "Cập nhật chất liệu thành công", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
