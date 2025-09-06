using API.Domain.Request.ImageRequest;
using API.Domain.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]

    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IProductDetailService _productDetailService;

        public ImageController(IImageService imageService, IProductDetailService productDetailService)
        {
            _imageService = imageService;
            _productDetailService = productDetailService;
        }

        [HttpPost("upload-multiple")]
        [RequestSizeLimit(10_000_000)] // 10MB nếu cần giới hạn
        public async Task<IActionResult> Upload([FromForm] UploadMultipleImagesRequest request)
        {
            var result = await _imageService.UploadMultipleImagesAsync(request);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromForm] UpdateImageRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var success = await _imageService.UpdateImageAsync(request);
                return Ok(new { Success = success });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // Cập nhật API SetMain để nhận thêm productDetailId, kiểm tra hợp lệ rồi set main
        [HttpPut("set-main")]
        public async Task<IActionResult> SetMain([FromBody] SetMainImageRequest request)
        {
            if (request == null || request.ImageId == Guid.Empty || request.ProductDetailId == Guid.Empty)
                return BadRequest(new { Message = "ImageId và ProductDetailId phải được cung cấp." });

            try
            {
                var success = await _imageService.SetMainImageAsync(request.ImageId, request.ProductDetailId);
                return Ok(new { Success = success });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // Lấy tất cả ảnh
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _imageService.GetAllAsync();
            return Ok(result);
        }

        // Lấy ảnh theo id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _imageService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // Lấy tất cả ảnh theo ProductDetailId
        [HttpGet("product-detail/{productDetailId}")]
        public async Task<IActionResult> GetImagesByProductDetailId(Guid productDetailId)
        {
            if (productDetailId == Guid.Empty)
                return BadRequest(new { Message = "ProductDetailId không hợp lệ." });

            var images = await _imageService.GetImagesByProductDetailIdAsync(productDetailId);
            return Ok(images);
        }
        // Xóa tất cả ảnh của ProductDetail theo ProductDetailId
        [HttpDelete("product-detail/{productDetailId}")]
        public async Task<IActionResult> DeleteAllByProductDetailId(Guid productDetailId)
        {
            if (productDetailId == Guid.Empty)
                return BadRequest(new { Message = "ProductDetailId không hợp lệ." });

            try
            {
                var success = await _imageService.DeleteAllImagesByProductDetailIdAsync(productDetailId);
                return Ok(new { Success = success, Message = "Đã xóa tất cả ảnh của ProductDetail." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

    }
}
