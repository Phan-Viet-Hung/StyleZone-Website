using API.DomainCusTomer.DTOs.Nam;
using API.DomainCusTomer.DTOs.Nu;
using API.DomainCusTomer.Request.Nu;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Web;

namespace MVC.Controllers
{
    public class NuCustomerController : Controller
    {
        private readonly HttpClient _httpClient;

        //private readonly string 
        public NuCustomerController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        // GET: NuCustomerController
        [HttpGet]
        public async Task<IActionResult> DoNuCustomer([FromQuery] DoNuCustomerFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/DoNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageDoNuCustome>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AoNu([FromQuery] AoNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/AoNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageAoNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AoTShirtNu([FromQuery] AoTShirtNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/AoTShirtNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageAoTShirtNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AoPoLoNu([FromQuery] AoPoLoNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/AoPoLoNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageAoPoLoNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AoGioNu([FromQuery] AoGioNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/AoGioNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageAoGioNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AoNiNu([FromQuery] AoNiNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/AoNiNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageAoNiNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AoDaiTayNu([FromQuery] AoDaiTayNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/AoDaiTayNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageAoDaiTayNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AoLongVuNu([FromQuery] AoLongVuNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/AoLongVuNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageAoLongVuNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> QuanNu([FromQuery] QuanNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/QuanNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageQuanNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> QuanShortNu([FromQuery] QuanShortNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/QuanShortNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageQuanShortNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> QuanGioNu([FromQuery] QuanGioNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/QuanGioNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageQuanGioNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> QuanNiNu([FromQuery] QuanNiNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/QuanNiNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageQuanNiNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GiayNu([FromQuery] GiayNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/GiayNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageGiayNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GiayThoiTrangNu([FromQuery] GiayThoiTrangNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/GiayThoiTrangNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageGiayThoiTrangNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GiayChayBoNu([FromQuery] GiayChayBoNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/GiayChayBoNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageGiayChayBoNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GiayCauLongNu([FromQuery] GiayCauLongNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/GiayCauLongNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageGiayCauLongNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GiayBongRoNu([FromQuery] GiayBongRoNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/GiayBongRoNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageGiayBongRoNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GiayBongDaNu([FromQuery] GiayBongDaNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/GiayBongDaNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageGiayBongDaNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> BoQuanAoNu([FromQuery] BoQuanAoNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/BoQuanAoNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageBoQuanAoNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> BoQuanAoBongRoNu([FromQuery] BoQuanAoBongRoNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/BoQuanAoBongRoNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageBoQuanAoBongRoNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> BoQuanAoCauLongNu([FromQuery] BoQuanAoCauLongNuFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            // Tạo query string
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
            {
                foreach (var item in filterRequest.Product.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Product", item);
            }

            if (filterRequest.Colors != null)
            {
                foreach (var color in filterRequest.Colors.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Colors", color);
            }

            if (filterRequest.Sizes != null)
            {
                foreach (var size in filterRequest.Sizes.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Sizes", size);
            }

            if (filterRequest.Genders != null)
            {
                foreach (var gender in filterRequest.Genders.Where(x => !string.IsNullOrWhiteSpace(x)))
                    query.Add("Genders", gender);
            }
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            // Tạo URL gọi API
            var url = $"https://localhost:7257/api/NuCustomer/BoQuanAoCauLongNu?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PageBoQuanAoCauLongNu>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }


        // GET: NuCustomerController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: NuCustomerController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: NuCustomerController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: NuCustomerController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: NuCustomerController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: NuCustomerController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: NuCustomerController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
