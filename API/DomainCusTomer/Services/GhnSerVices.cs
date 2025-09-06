using API.DomainCusTomer.Request.GHN;
using API.DomainCusTomer.Services.IServices;
using System.Text.Json;

namespace API.DomainCusTomer.Services
{
    public class GhnSerVices : IGhnService
    {
        private readonly HttpClient _client;
        private const string Token = "3c6d36d0-ea92-11ef-a839-66afa442234f";
        private const string ShopId = "5634551";

        public GhnSerVices(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri("https://online-gateway.ghn.vn/shiip/public-api/");
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Token", Token);
            _client.DefaultRequestHeaders.Add("ShopId", ShopId);
        }
        public async Task<decimal> CalculateFeeAsync(ShippingFeeRequest request)
        {
            // Đảm bảo from_district_id luôn có giá trị mặc định
            if (request.from_district_id == 0) request.from_district_id = 1450;

            var response = await _client.PostAsJsonAsync("v2/shipping-order/fee", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"GHN API Error: {response.StatusCode} - {errorContent}");
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("data").GetProperty("total").GetDecimal();
        }

        public async Task<decimal> GetShippingFeeAsync(ShippingFeeRequest request)
        {
            return await CalculateFeeAsync(request);
        }

        /// <summary>
        /// Lấy danh sách Tỉnh/Thành phố từ GHN
        /// </summary>
        public async Task<string> GetProvincesAsync()
        {
            var response = await _client.GetAsync("master-data/province");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"GHN API Error: {response.StatusCode} - {errorContent}");
            }
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Lấy danh sách Quận/Huyện từ GHN
        /// </summary>
        public async Task<string> GetDistrictsAsync(int provinceId)
        {
            var content = JsonContent.Create(new { province_id = provinceId });
            var response = await _client.PostAsync("master-data/district", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"GHN API Error: {response.StatusCode} - {errorContent}");
            }
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Lấy danh sách Phường/Xã từ GHN
        /// </summary>
        public async Task<string> GetWardsAsync(int districtId)
        {
            var content = JsonContent.Create(new { district_id = districtId });
            var response = await _client.PostAsync("master-data/ward", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"GHN API Error: {response.StatusCode} - {errorContent}");
            }
            return await response.Content.ReadAsStringAsync();
        }
        public async Task<int> GetAvailableServicesAsync(int toDistrictId)
        {
            var content = JsonContent.Create(new
            {
                shop_id = int.Parse(ShopId),
                from_district = 1482,
                to_district = toDistrictId
            });

            var response = await _client.PostAsync("v2/shipping-order/available-services", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"GHN API Error: {response.StatusCode} - {errorContent}");
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var services = json.GetProperty("data").EnumerateArray();
            if (!services.Any()) return 0;

            return services.First().GetProperty("service_id").GetInt32();
        }
    }
}
