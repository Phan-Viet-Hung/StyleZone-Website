using Microsoft.AspNetCore.Mvc;
using API.DomainCusTomer.DTOs.ThongTinCaNhaCustomer;
using API.DomainCusTomer.Services.IServices;
using System.Threading.Tasks;
using DAL_Empty.Models;
using API.DomainCusTomer.Request.ThongTinCaNhan;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonMuaCustomerController : ControllerBase
    {
        private readonly IDonMuaCustomerServices _donMuaCustomerService;

        public DonMuaCustomerController(IDonMuaCustomerServices donMuaCustomerService)
        {
            _donMuaCustomerService = donMuaCustomerService;
        }
        [HttpPost("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(Guid orderId, string username, string Decription)
        {
            try
            {
                await _donMuaCustomerService.CancelOrderAsync(orderId, username, Decription);

                return Ok(new
                {
                    Success = true,
                    Message = "Đơn hàng đã được hủy thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        [HttpPost("cancelGuest/{orderId}")]
        public async Task<IActionResult> CancelOrderGuest(Guid orderId, string Decription)
        {
            try
            {
                await _donMuaCustomerService.CancelOrderAsyncGuest(orderId, Decription);

                return Ok(new
                {
                    Success = true,
                    Message = "Đơn hàng đã được hủy thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        // Lấy danh sách đơn hàng của khách hàng
        [HttpGet("all/{username}")]
        public async Task<IActionResult> GetAllOrders(string username)
        {
            var result = await _donMuaCustomerService.ListDonHang(username);
            return Ok(result);
        }

        [HttpGet("pending/{username}")]
        public async Task<IActionResult> GetPendingOrders(string username)
        {
            var result = await _donMuaCustomerService.ListDonHangPending(username);
            return Ok(result);
        }

        [HttpGet("confirmed/{username}")]
        public async Task<IActionResult> GetConfirmedOrders(string username)
        {
            var result = await _donMuaCustomerService.ListDonHangConfirmed(username);
            return Ok(result);
        }

        [HttpGet("processing/{username}")]
        public async Task<IActionResult> GetProcessingOrders(string username)
        {
            var result = await _donMuaCustomerService.ListDonHangProcessing(username);
            return Ok(result);
        }

        [HttpGet("shipping/{username}")]
        public async Task<IActionResult> GetShippingOrders(string username)
        {
            var result = await _donMuaCustomerService.ListDonHangShipping(username);
            return Ok(result);
        }

        [HttpGet("delivered/{username}")]
        public async Task<IActionResult> GetDeliveredOrders(string username)
        {
            var result = await _donMuaCustomerService.ListDonHangDelivered(username);
            return Ok(result);
        }

        [HttpGet("cancelled/{username}")]
        public async Task<IActionResult> GetCancelledOrders(string username)
        {
            var result = await _donMuaCustomerService.ListDonHangCancelled(username);
            return Ok(result);
        }


        [HttpGet("{username}/addresses")]
        public async Task<IActionResult> GetListCustomerAddresses(string username)
        {
            try
            {
                var addresses = await _donMuaCustomerService.ListDiaChiCustomer(username);

                if (addresses == null || addresses.Count == 0)
                    return NotFound("Không tìm thấy địa chỉ của khách hàng.");

                return Ok(addresses); // Trả về JSON danh sách địa chỉ
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // Thêm địa chỉ mới
        [HttpPost("{username}/address")]
        public async Task<IActionResult> AddAddress(string username, [FromBody] DiachiCustomerDto newAddress)
        {
            try
            {
                await _donMuaCustomerService.AddDiaChi(username, newAddress);
                return CreatedAtAction(nameof(AddAddress), new { username }, newAddress);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("UpdateStatusDiaChi/{username}/{id}")]
        public async Task<IActionResult> UpdateStatusDiaChi(string username, Guid id)
        {
            try
            {
                await _donMuaCustomerService.UpdateStastusDiaChi(id, username);
                return Ok(new { message = "Cập nhật địa chỉ mặc định thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // Cập nhật địa chỉ của khách hàng
        [HttpPut("address/{id}")]
        public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] DiachiCustomerDto newAddress)
        {
            try
            {
                await _donMuaCustomerService.UpdateDiaChi(id, newAddress);
                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi cập nhật địa chỉ", error = ex.Message });
            }
        }
        [HttpGet("{username}")]
        public async Task<IActionResult> GetCustomerByUsername(string username)
        {
            var customer = await _donMuaCustomerService.DetailsCustome(username);

            if (customer == null)
                return NotFound("Không tìm thấy khách hàng.");

            return Ok(customer);
        }
        // Cập nhật thông tin khách hàng
        [HttpPost("{username}/update")]
        public async Task<IActionResult> UpdateCustomer(string username, [FromBody] ThongTinCaNhanRequest updatedCustomer)
        {
            try
            {
                await _donMuaCustomerService.UpdateCustome(username, updatedCustomer);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Thay đổi mật khẩu
        [HttpPost("{username}/change-password")]
        public async Task<IActionResult> ChangePassword(string username, [FromBody] RePassDtoCustomer rePassDtoCustomer)
        {
            if (username != username)
            {
                return BadRequest("Tên người dùng không khớp.");
            }

            try
            {
                await _donMuaCustomerService.UpdatePassWord(rePassDtoCustomer, username);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("remove/{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            try
            {
                await _donMuaCustomerService.RemoveDiaChi(id);
                return Ok(new { message = "Xóa địa chỉ thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Đã xảy ra lỗi khi xóa.", error = ex.Message });
            }
        }

    }
}
