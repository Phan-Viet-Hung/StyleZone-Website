using API.Domain.DTOs;
using API.Domain.Request.ProductDetailRequest;
using API.Domain.Request.ProductRequest;
using API.Domain.Service.IService;
using DAL_Empty.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductDto>>> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetById(Guid id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create([FromForm] CreateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate cơ bản
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Tên sản phẩm không được để trống." });

            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 500)
                return BadRequest(new { message = "Mô tả không được vượt quá 500 ký tự." });

            if (request.CategoryId == Guid.Empty)
                return BadRequest(new { message = "Chọn danh mục sản phẩm hợp lệ." });

            if (request.BrandId == Guid.Empty)
                return BadRequest(new { message = "Chọn thương hiệu sản phẩm hợp lệ." });

            if (!Enum.IsDefined(typeof(GenderEnum), request.Gender))
                return BadRequest(new { message = "Giới tính sản phẩm không hợp lệ." });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Không lấy được thông tin người dùng." });

            try
            {
                // ✅ Service đã check trùng tên + danh mục
                var createdProduct = await _productService.CreateAsync(request, userId);
                return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                // Trả về JSON lỗi giống ProductService
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id == Guid.Empty)
                return BadRequest(new { message = "ID sản phẩm không hợp lệ." });

            // Validate cơ bản
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Tên sản phẩm không được để trống." });

            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 500)
                return BadRequest(new { message = "Mô tả không được vượt quá 500 ký tự." });

            if (request.CategoryId == Guid.Empty)
                return BadRequest(new { message = "Chọn danh mục sản phẩm hợp lệ." });

            if (request.BrandId == Guid.Empty)
                return BadRequest(new { message = "Chọn thương hiệu sản phẩm hợp lệ." });

            if (!Enum.IsDefined(typeof(GenderEnum), request.Gender))
                return BadRequest(new { message = "Giới tính sản phẩm không hợp lệ." });

            request.Id = id;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Không lấy được thông tin người dùng." });

            try
            {
                // ✅ Service đã check trùng tên + danh mục ngoại trừ chính nó
                var result = await _productService.UpdateAsync(request, userId);
                if (result == null)
                    return NotFound(new { message = "Không tìm thấy sản phẩm." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Trả về JSON lỗi giống ProductService
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}
