using API.DomainCusTomer.Request.GHN;
using System.Text.Json;

namespace API.DomainCusTomer.Services.IServices
{
    public interface IGhnService
    {
        Task<decimal> CalculateFeeAsync(ShippingFeeRequest request);
        Task<decimal> GetShippingFeeAsync(ShippingFeeRequest request);

        Task<string> GetProvincesAsync();
        Task<string> GetDistrictsAsync(int provinceId);
        Task<string> GetWardsAsync(int districtId);
        Task<int> GetAvailableServicesAsync(int toDistrictId);
    }
}
