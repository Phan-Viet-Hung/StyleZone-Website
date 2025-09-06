using DAL_Empty.Models;

namespace API.DomainCusTomer.DTOs.Nu
{
    public class PageAoNiNu
    {
        public List<ProductDetailCustomerDto> Items { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public List<string> Categories { get; set; } = new();
        public List<string> Sizes { get; set; } = new();
        public List<string> Colors { get; set; } = new();
        public List<GenderEnum> Genders { get; set; } = new();
    }
}
