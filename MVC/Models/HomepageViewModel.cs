using API.DomainCusTomer.DTOs.TrangChu;

namespace MVC.Models
{
    public class HomepageViewModel
    {
      
        public Dictionary<string, List<HomeProductCustomerDto>> FeaturedProducts { get; set; }

        // Thêm thuộc tính này vào nếu nó đang bị thiếu
        public List<HomeProductCustomerDto> Promotions { get; set; }

        public HomepageViewModel()
        {
            FeaturedProducts = new Dictionary<string, List<HomeProductCustomerDto>>();
            Promotions = new List<HomeProductCustomerDto>();
        }
    }
}
