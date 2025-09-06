using DAL_Empty.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DomainCusTomer.DTOs.CastCustomerId
{
    public class CastCustomerIDDto
    {
        public Guid Id { get; set; }
      
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public string productdetailcode {  get; set; }
        public Guid? ProductDetailId { get; set; }
        public string ProductName { get; set; }
        public string ColorName { get; set; }
        public string SizeName { get; set; }
        public string ImageUrl { get; set; }
        public int? StockQuantity { get; set; }
    }
}
