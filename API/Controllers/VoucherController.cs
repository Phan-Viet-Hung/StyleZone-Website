using API.Domain.Request.VoucherRequest;
using API.Domain.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        private IActionResult ProcessModelStateErrors()
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage).ToList();

            var errorsVi = errors.Select(err =>
            {
                if (err.Contains("is not valid") || err.Contains("The value") && err.Contains("is invalid"))
                    return "Giá trị nhập không hợp lệ, vui lòng kiểm tra lại kiểu dữ liệu.";
                if (err.Contains("must be a number"))
                    return "Trường này phải là số hợp lệ.";
                if (err.Contains("The field") && err.Contains("must be a valid date"))
                    return "Trường này phải là ngày hợp lệ.";
                return err;
            }).ToList();

            return BadRequest(new { Errors = errorsVi });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] CreateVoucherRequest request)
        {
            if (!ModelState.IsValid)
                return ProcessModelStateErrors();

            try
            {
                var result = await _voucherService.CreateAsync(request);
                return Ok(new { message = "Tạo voucher thành công", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateVoucherRequest request)
        {
            if (!ModelState.IsValid)
                return ProcessModelStateErrors();

            try
            {
                var result = await _voucherService.UpdateAsync(request);
                return Ok(new { message = "Cập nhật voucher thành công", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            try
            {
                var result = await _voucherService.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = "Không tìm thấy voucher" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var result = await _voucherService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/change-status")]
        public async Task<IActionResult> ChangeStatusForm(Guid id, [FromForm] string status)
        {
            try
            {
                var request = new ChangeStatusRequest { Id = id, Status = status };
                var result = await _voucherService.ChangeStatusAsync(request);
                if (result)
                    return Ok(new { message = "Thay đổi trạng thái thành công" });
                else
                    return BadRequest(new { message = "Thay đổi trạng thái thất bại" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("bulk-change-status")]
        public async Task<IActionResult> BulkChangeStatus([FromForm] BulkStatusChangeRequest request)
        {
            if (!ModelState.IsValid)
                return ProcessModelStateErrors();

            try
            {
                var result = await _voucherService.BulkChangeStatusAsync(request);
                if (result)
                    return Ok(new { message = "Thay đổi trạng thái hàng loạt thành công" });
                else
                    return BadRequest(new { message = "Thay đổi trạng thái hàng loạt thất bại" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
