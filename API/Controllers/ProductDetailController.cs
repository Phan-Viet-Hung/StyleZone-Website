using API.Domain.DTOs;
using API.Domain.Request.ProductDetailRequest;
using API.Domain.Request.VoucherRequest;
using API.Domain.Service;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/product-details")]
    public class ProductDetailController : ControllerBase
    {
        private readonly IProductDetailService _service;
        private readonly IImageService _imageService;
        private readonly DbContextApp _context;

        public ProductDetailController(IProductDetailService service, IImageService IImageService,DbContextApp context)
        {
            _service = service;
            _imageService = IImageService;
            _context = context;
        }

        // GET: api/product-details?productId=...
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDetailDto>>> GetAll([FromQuery] Guid? productId)
        {
            if (productId != null && productId == Guid.Empty)
            {
                return BadRequest(new { Message = "ProductId không hợp lệ." });
            }

            var result = await _service.GetAllAsync(productId);
            return Ok(result);
        }

        // GET: api/product-details/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDetailDto>> GetById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { Message = "Id không hợp lệ." });

            var detail = await _service.GetByIdAsync(id);
            if (detail == null)
                return NotFound(new { Message = "Chi tiết sản phẩm không tồn tại." });

            return Ok(detail);
        }

        // POST: api/product-details
        [HttpPost]
        public async Task<ActionResult<ProductDetailDto>> Create([FromForm] CreateProductDetailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _service.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Tạo chi tiết sản phẩm thất bại.", Detail = ex.Message });
            }
        }

        // PUT: api/product-details/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDetailDto>> Update(Guid id, [FromForm] UpdateProductDetailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id == Guid.Empty || id != request.Id)
                return BadRequest(new { Message = "ID không hợp lệ hoặc không khớp với dữ liệu gửi lên." });

            try
            {
                var updated = await _service.UpdateAsync(request);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = "Không tìm thấy chi tiết sản phẩm để cập nhật." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Cập nhật thất bại.", Detail = ex.Message });
            }
        }
        [HttpPut("change-status")]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusRequest request)
        {
            try
            {
                var result = await _service.ChangeStatusAsync(request);
                return Ok(new { success = result, message = "Cập nhật trạng thái thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("bulk-change-status")]
        public async Task<IActionResult> BulkChangeStatus([FromBody] BulkStatusChangeRequest request)
        {
            try
            {
                var result = await _service.BulkChangeStatusAsync(request);
                return Ok(new { success = result, message = "Cập nhật trạng thái hàng loạt thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        // GET: api/product-details/by-ids?ids=guid1&ids=guid2&ids=guid3
        [HttpGet("by-ids")]
        public async Task<ActionResult<IEnumerable<ProductDetailDto>>> GetByIds([FromQuery] List<Guid> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(new { Message = "Danh sách ID sản phẩm không hợp lệ." });
            }

            var results = await _service.GetByIdsAsync(ids);

            return Ok(results);
        }
        [HttpPost("full")]
        [DisableRequestSizeLimit] // Optional: cho phép upload file lớn
        public async Task<IActionResult> CreateFullProductDetail([FromForm] FullCreateProductDetailRequest request)
        {
            try
            {
                // 1. Tạo chi tiết sản phẩm
                var created = await _service.CreateAsync(request.ProductDetail);

                // 2. Upload ảnh nếu có
                if (request.Images.Any())
                {
                    foreach (var imageFile in request.Images)
                    {
                        if (imageFile.Length > 0)
                        {
                            using var ms = new MemoryStream();
                            await imageFile.CopyToAsync(ms);
                            var bytes = ms.ToArray();

                            // Generate file name and path
                            var fileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
                            var imagePath = Path.Combine("wwwroot", "uploads", fileName); // Thay đổi theo cấu trúc server

                            // Lưu file vào server
                            await System.IO.File.WriteAllBytesAsync(imagePath, bytes);

                            // Tạo bản ghi ảnh trong DB
                            var imageEntity = new Image
                            {
                                Id = Guid.NewGuid(),
                                FileName = imageFile.FileName,
                                Url = $"/uploads/{fileName}",
                                ProductDetailId = created.Id,
                                IsMainImage = imageFile.FileName == request.MainImageFileName,
                                CreatedAt = DateTime.Now
                            };

                            _context.Images.Add(imageEntity);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("with-price")]
        public async Task<IActionResult> GetProductDetailsWithPrice(Guid? productId = null)
        {
            var result = await _service.GetAllWithDisplayPriceAsync(productId);
            return Ok(result);
        }
        // GET: api/product-details/available-for-promotion?promotionId=...
        [HttpGet("available-for-promotion")]
        public async Task<ActionResult<IEnumerable<ProductDetailDto>>> GetAvailableForPromotion([FromQuery] Guid? promotionId)
        {
            try
            {
                var products = await _service.GetAvailableForPromotionAsync(promotionId);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lấy danh sách sản phẩm khả dụng thất bại.", Detail = ex.Message });
            }
        }
        // GET: api/product-details/by-promotion?promotionId=...
        [HttpGet("by-promotion")]
        public async Task<ActionResult<IEnumerable<ProductDetailDto>>> GetByPromotion([FromQuery] Guid promotionId)
        {
            if (promotionId == Guid.Empty)
                return BadRequest(new { Message = "PromotionId không hợp lệ." });

            try
            {
                var products = await _service.GetByPromotionIdAsync(promotionId);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lấy danh sách sản phẩm trong promotion thất bại.", Detail = ex.Message });
            }
        }
        [HttpPost("import-excel")]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> ImportFromExcel([FromForm] ImportExcelProductDetailRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest(new { Message = "File Excel không được để trống." });

            if (request.ProductId == Guid.Empty)
                return BadRequest(new { Message = "ProductId không hợp lệ." });

            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xlsx");

            try
            {
                using (var stream = System.IO.File.Create(tempFile))
                {
                    await request.File.CopyToAsync(stream);
                }

                string result = await (_service as ProductDetailService)!
                    .ImportProductDetailFromExcelAsync(tempFile, request.ProductId);

                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Import thất bại.", Detail = ex.Message });
            }
            finally
            {
                if (System.IO.File.Exists(tempFile))
                    System.IO.File.Delete(tempFile);
            }
        }

    }

}

