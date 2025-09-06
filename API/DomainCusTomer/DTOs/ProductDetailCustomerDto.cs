using API.DomainCusTomer.DTOs.DetailCustomer;
using DAL_Empty.Models;

namespace API.DomainCusTomer.DTOs
{
    public class ProductDetailCustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string CodeProductDetail { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductDescription { get; set; }
        public GenderEnum Gender { get; set; }
        public string GenderName => Gender switch
        {
            GenderEnum.Nam => "Nam",
            GenderEnum.Nu => "Nữ",
            GenderEnum.Khac => "Khác"
        };
        public DateTime DateTime { get; set; }

        public Guid? ColorId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string ColorCode {  get; set; } = string.Empty;

        public Guid? SizeId { get; set; }
        public string Code { get; set; } = string.Empty;

        public Guid? MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;

        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public Guid? BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;

        public Guid? OriginId { get; set; }
        public string OriginName { get; set; } = string.Empty;

         public Guid? SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;

        public List<string> PromotionId { get; set; } = new();
        public List<string> PromotionName { get; set; } = new();
        public List<string> StartDate { get; set; } = new();
        public List<string> EndDate { get; set; } = new();
        public List<VoucherStatus> Status { get; set; } = new();

        public List<Guid?> PromotionProductId { get; set; } = new();
        public List<Decimal> PriceBeforeReduction { get; set; } = new();
        public List<Decimal> Priceafterduction { get; set; } = new();

        //public List<string> ColorCode { get; set; } = new();
        public List<string> SizeCode { get; set; } = new();

        public List<PageDetail> ProductDetailsSameCategory { get; set; }

        public List<string> ImageUrls { get; set; } = new();
    }
}
