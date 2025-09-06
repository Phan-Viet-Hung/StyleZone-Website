using API.Domain.DTOs;
using API.Domain.Request.PromotionRequest;
using API.Domain.Request.VoucherRequest;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] CreatePromotionRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Dữ liệu không hợp lệ", errors });
            }

            try
            {
                var result = await _promotionService.CreateAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdatePromotionRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Dữ liệu không hợp lệ", errors });
            }

            try
            {
                var result = await _promotionService.UpdateAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _promotionService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _promotionService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy chương trình khuyến mãi." });

            return Ok(result);
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetDetailById(Guid id)
        {
            var result = await _promotionService.GetDetailByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy chương trình khuyến mãi." });

            return Ok(result);
        }

        
        [HttpGet("products")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _promotionService.GetAllProductsAsync();
            return Ok(products);
        }
        [HttpPut("change-status")]
        public async Task<IActionResult> ChangeStatus([FromForm] ChangeStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Dữ liệu không hợp lệ", errors });
            }

            try
            {
                if (!Enum.TryParse<VoucherStatus>(request.Status, true, out var newStatus))
                    return BadRequest(new { message = "Trạng thái không hợp lệ." });

                await _promotionService.ChangePromotionStatusAsync(request.Id, newStatus);

                return Ok(new { message = "Cập nhật trạng thái thành công." });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Không tìm thấy"))
                    return NotFound(new { message = ex.Message });

                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPut("change-status-bulk")]
        public async Task<IActionResult> BulkChangeStatus([FromForm] BulkStatusChangeRequest request)
        {
            if (!ModelState.IsValid)
                return ProcessModelStateErrors();

            try
            {
                var success = await _promotionService.BulkChangePromotionStatusAsync(request);
                if (success)
                    return Ok(new { message = "Thay đổi trạng thái hàng loạt thành công" });
                else
                    return BadRequest(new { message = "Thay đổi trạng thái hàng loạt thất bại" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
    }

}
