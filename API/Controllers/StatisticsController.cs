using API.Domain.DTOs.ThongKe;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticService _statisticService;

        public StatisticsController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        [HttpPost("dashboard")]
        public async Task<IActionResult> GetDashboardStatistics([FromBody] DateFilterDto filter)
        {
            try
            {
                var result = await _statisticService.GetDashboardStatisticsAsync(filter);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        [HttpPost("top-brands")]
        public async Task<IActionResult> GetTopBrands([FromBody] DateFilterDto filter, [FromQuery] int top = 3)
        {
            try
            {
                if (top <= 0)
                {
                    return BadRequest(new
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = "Giá trị 'top' phải lớn hơn 0."
                    });
                }

                var result = await _statisticService.GetTopBrandsAsync(filter, top);

                if (result == null || !result.Any())
                {
                    return Ok(new
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Data = new List<TopBrandDto>(),
                        Message = "Không có dữ liệu thương hiệu trong khoảng thời gian này."
                    });
                }

                return Ok(new
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Đã xảy ra lỗi: " + ex.Message
                });
            }
        }

        [HttpPost("top-products")]
        public async Task<IActionResult> GetTopProducts([FromBody] DateFilterDto filter, [FromQuery] int top = 10)
        {
            try
            {
                if (top <= 0)
                {
                    return BadRequest(new
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = "Giá trị 'top' phải lớn hơn 0."
                    });
                }

                var result = await _statisticService.GetTopProductsAsync(filter, top);

                if (result == null || !result.Any())
                {
                    return Ok(new
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Data = new List<TopProductDto>(),
                        Message = "Không có dữ liệu sản phẩm trong khoảng thời gian này."
                    });
                }

                return Ok(new
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Đã xảy ra lỗi: " + ex.Message
                });
            }
        }

       
        [HttpPost("order-status-statistics")]
        public async Task<IActionResult> GetOrderStatusStatistics([FromBody] DateFilterDto filter)
        {
            try
            {
                var result = await _statisticService.GetOrderStatusStatisticsAsync(filter);

                return Ok(new
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Đã xảy ra lỗi: " + ex.Message
                });
            }
        }
    }
}