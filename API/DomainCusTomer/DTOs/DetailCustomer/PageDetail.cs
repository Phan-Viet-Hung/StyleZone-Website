using DAL_Empty.Models;

namespace API.DomainCusTomer.DTOs.DetailCustomer
{
    public class PageDetail
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public string ImageUrl { get; set; }
    }
}
