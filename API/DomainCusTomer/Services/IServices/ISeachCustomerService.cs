using API.DomainCusTomer.DTOs.SeachCustomer;

namespace API.DomainCusTomer.Services.IServices
{
    public interface ISeachCustomerService
    {
        Task<List<ProductSearchResultDto>> SearchProductsAsync(string? keyword);
    }
}
