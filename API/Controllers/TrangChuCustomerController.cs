using API.DomainCusTomer.DTOs.TrangChu;
using API.DomainCusTomer.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrangChuCustomerController : Controller
    {
        private readonly ITrangChuCustomerService _trangChuService;
        private readonly ITinTucService _tinTucService;
        public TrangChuCustomerController(ITrangChuCustomerService trangChuService, ITinTucService tinTucService)
        {
            _trangChuService = trangChuService;
            _tinTucService = tinTucService;
        }
        [HttpGet("SanPhamTrangChu")]
        public async Task<IActionResult> SanPhamTrangChu()
        {
            var result = await _trangChuService.GetSanPhamTrangChu();
            return Ok(result);
        }

        [HttpGet("TinTucTrangChu")]
        public async Task<IActionResult> TinTucTrangChu()
        {
            var news = await _tinTucService.GetAllTinTucAsync();
            var latestNews = news.OrderByDescending(x => x.CreatedDate).Take(3).ToList();

            var result = latestNews.Select(x => new HomeProductCustomerDto
            {
                Id = x.Id,
                Name = x.Title,
                ImageUrl = x.ImageUrl,
                ImageUrlHover = x.ImageUrl,
                Price = 0,
                DiscountPrice = null,
                CategoryName = "Tin tức",
                ShortDescription = x.ShortDescription
            }).ToList();

            return Ok(result);
        }
    }
}
