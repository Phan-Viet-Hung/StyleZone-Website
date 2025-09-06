using API.DomainCusTomer.DTOs.Tintuc;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MVC.Controllers
{
    public class TinTucCustomerController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://localhost:7257/api/TinTucCustomer"; 

        public TinTucCustomerController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync(_apiUrl);
            var tinTucs = new List<TinTucDto>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                tinTucs = JsonConvert.DeserializeObject<List<TinTucDto>>(json);
            }
            return View(tinTucs);
        }

        public async Task<IActionResult> Detail(Guid id)
        {
            string detailApiUrl = $"{_apiUrl}/{id}";
            var response = await _httpClient.GetAsync(detailApiUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var tinTucDetail = JsonConvert.DeserializeObject<TinTucDetailDto>(json);
                return View(tinTucDetail);
            }
            return NotFound();
        }
    }
}
