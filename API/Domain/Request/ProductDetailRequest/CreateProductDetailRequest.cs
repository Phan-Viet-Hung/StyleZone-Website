using API.Domain.Request.ImageRequest;
using DAL_Empty.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace API.Domain.Request.ProductDetailRequest
{
    public class CreateProductDetailRequest
    {
        [Required(ErrorMessage = "Code sản phẩm là bắt buộc")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "Sản phẩm là bắt buộc")]
        public Guid ProductId { get; set; }
        [Required(ErrorMessage = "Màu là bắt buộc")]
        public Guid? ColorId { get; set; }
        [Required(ErrorMessage = "Kích thước là bắt buộc")]
        public Guid? SizeId { get; set; }
        [Required(ErrorMessage = "Chất liệu là bắt buộc")]
        public Guid? MaterialId { get; set; }
        [Required(ErrorMessage = "Xuất xứ là bắt buộc")]
        public Guid? OriginId { get; set; }
        [Required(ErrorMessage = "Nhà cung cấp là bắt buộc")]
        public Guid? SupplierId { get; set; }

        public ProductDetailStatus Status { get; set; } = ProductDetailStatus.Active;
        public IEnumerable<IFormFile>? ImageFiles { get; set; }
        public int? MainImageIndex { get; set; }

    }
}