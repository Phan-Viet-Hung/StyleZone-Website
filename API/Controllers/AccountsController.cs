using System.Security.Claims;
using API.Domain.DTOs;
using API.Domain.Request.AccountRequest;
using API.Domain.Service.IService;
using API.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]

    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // Tạo tài khoản
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] List<CreateAccountRequest> requests)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var accounts = await _accountService.CreateAccountsAsync(requests);
                return Ok(new
                {
                    Message = "Tạo tài khoản thành công",
                    Data = accounts
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // Cập nhật tài khoản
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _accountService.UpdateAccountAsync(id, request);
                return Ok(new
                {
                    Message = "Cập nhật thành công",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống", Error = ex.Message });
            }
        }

        // Lấy toàn bộ tài khoản
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _accountService.GetAllAccountsAsync();
            return Ok(data);
        }

        // Lấy theo ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var acc = await _accountService.GetByIdAsync(id);
            if (acc == null)
                return NotFound(new { Message = $"Không tìm thấy tài khoản với ID {id}" });

            return Ok(acc);
        }

        // Lấy theo số điện thoại
        [HttpGet("by-phone/{phoneNumber}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByPhone(string phoneNumber)
        {
            var acc = await _accountService.GetByPhoneNumberAsync(phoneNumber);
            if (acc == null)
                return NotFound(new { Message = $"Không tìm thấy tài khoản với số {phoneNumber}" });

            return Ok(acc);
        }

        [HttpPost("import-excel")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportExcel([FromForm] ImportExcelRequest request)
        {
            try
            {
                var result = await _accountService.ImportAccountsFromExcelAsync(request.File);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi xử lý file", Error = ex.Message });
            }
        }


        // Toggle trạng thái active
        [HttpPut("{id}/toggle-active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Toggle(Guid id)
        {
            try
            {
                var success = await _accountService.ToggleActiveStatusAsync(id);
                if (!success)
                    return NotFound(new { Message = $"Không tìm thấy tài khoản với ID {id}" });

                return Ok(new { Message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống", Error = ex.Message });
            }
        }

        // Cập nhật trạng thái hàng loạt
        [HttpPut("bulk-set-active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetBulkActive([FromBody] List<SetActiveStatusRequest> requests)
        {
            try
            {
                var success = await _accountService.SetActiveStatusesAsync(requests);
                return Ok(new { Message = "Đã cập nhật trạng thái hàng loạt", Data = success });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi cập nhật trạng thái", Error = ex.Message });
            }
        }

        // Lấy tất cả role
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _accountService.GetAllRolesAsync();
            return Ok(roles);
        }
        [HttpGet("profile")]

        [Authorize]
        public async Task<ActionResult<AccountDto>> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var account = await _accountService.GetByIdAsync(Guid.Parse(userId));
            if (account == null) return NotFound();

            return Ok(account);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var updated = await _accountService.UpdateProfileAsync(Guid.Parse(userId), request);
            if (!updated) return BadRequest("Cập nhật thất bại!");

            return Ok(new { message = "Cập nhật thành công" });
        }
    }
}