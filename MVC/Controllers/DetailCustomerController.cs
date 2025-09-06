using API.DomainCusTomer.DTOs;
using API.DomainCusTomer.DTOs.DetailCustomer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace MVC.Controllers
{
    public class DetailCustomerController : Controller
    {
        private readonly HttpClient _httpClient;

        //private readonly string 
        public DetailCustomerController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        // GET: DetailCustomerController
        public ActionResult Index()
        {
            return View();
        }

        // GET: DetailCustomerController/Details/5
        [HttpGet]
        public async Task<IActionResult> DetailCustomer(Guid id)
        {
            var response = await _httpClient.GetAsync($"https://localhost:7257/api/DetailCustomer/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }
            var json = await response.Content.ReadAsStringAsync();
            var productDetail = JsonConvert.DeserializeObject<DetailCustomerDto>(json);
            if (productDetail == null)
            {
                return NotFound();
            }
            return View(productDetail);
        }
        [HttpGet]
        public async Task<IActionResult> DetailCustomerID(Guid id)
        {
            var response = await _httpClient.GetAsync($"https://localhost:7257/api/DetailCustomer/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }
            var json = await response.Content.ReadAsStringAsync();
            var productDetail = JsonConvert.DeserializeObject<DetailCustomerDto>(json);
            if (productDetail == null)
            {
                return NotFound();
            }
            return View(productDetail);
        }
    }
}
