using API.DomainCusTomer.DTOs;
using API.DomainCusTomer.DTOs.TheThao;
using API.DomainCusTomer.Request.TheThao;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Web;

namespace MVC.Controllers
{
    public class TheThaoCustomerController : Controller
    {
        private readonly HttpClient _httpClient;
       
        //private readonly string 
        public TheThaoCustomerController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: TheThaoCustomer
        [HttpGet]
        public async Task<IActionResult> TheThaoCustomer([FromQuery] ProductFilterRequest filterRequest)
        {
            // Gán giá trị mặc định nếu thiếu
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
            var url = $"https://localhost:7257/api/TheThaoCustomer/TheThao?{query.ToString()}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                var pagedResult = JsonConvert.DeserializeObject<PagedProductResponse>(json);

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi ngoại lệ: {ex.Message}");
            }
        }




        [HttpGet]
        public async Task<IActionResult> PICKLEBALLCustomer([FromQuery] PickleballFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
                foreach (var item in filterRequest.Product)
                    query.Add("Product", item);

            if (filterRequest.Colors != null)
                foreach (var color in filterRequest.Colors)
                    query.Add("Colors", color);

            if (filterRequest.Sizes != null)
                foreach (var size in filterRequest.Sizes)
                    query.Add("Sizes", size);

            if (filterRequest.Genders != null)
                foreach (var gender in filterRequest.Genders)
                    query.Add("Genders", gender);
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());
            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            var url = $"https://localhost:7257/api/TheThaoCustomer/Pickleball/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PagePickleball>(json);


            return View(pagedResult);
        }

        [HttpGet]
        public async Task<IActionResult> ChayBoCustomer([FromQuery] ChayBoFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
                foreach (var item in filterRequest.Product)
                    query.Add("Product", item);

            if (filterRequest.Colors != null)
                foreach (var color in filterRequest.Colors)
                    query.Add("Colors", color);

            if (filterRequest.Sizes != null)
                foreach (var size in filterRequest.Sizes)
                    query.Add("Sizes", size);

            if (filterRequest.Genders != null)
                foreach (var gender in filterRequest.Genders)
                    query.Add("Genders", gender);
            if (!string.IsNullOrWhiteSpace(filterRequest.SortBy))
                query.Add("SortBy", filterRequest.SortBy.ToLower());

            if (!string.IsNullOrWhiteSpace(filterRequest.SortOrder))
                query.Add("SortOrder", filterRequest.SortOrder.ToLower());
            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            var url = $"https://localhost:7257/api/TheThaoCustomer/ChayBo/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PagaChayBo>(json);


            return View(pagedResult);
        }

        [HttpGet]
        public async Task<IActionResult> TapLuyenCustomer([FromQuery] TapLuyenFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
                foreach (var item in filterRequest.Product)
                    query.Add("Product", item);

            if (filterRequest.Colors != null)
                foreach (var color in filterRequest.Colors)
                    query.Add("Colors", color);

            if (filterRequest.Sizes != null)
                foreach (var size in filterRequest.Sizes)
                    query.Add("Sizes", size);

            if (filterRequest.Genders != null)
                foreach (var gender in filterRequest.Genders)
                    query.Add("Genders", gender);

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            var url = $"https://localhost:7257/api/TheThaoCustomer/TapLuyen/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageTapLuyen>(json);


            return View(pagedResult);
        }

        [HttpGet]
        public async Task<IActionResult> BongRoCustomer([FromQuery] BongRoFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
                foreach (var item in filterRequest.Product)
                    query.Add("Product", item);

            if (filterRequest.Colors != null)
                foreach (var color in filterRequest.Colors)
                    query.Add("Colors", color);

            if (filterRequest.Sizes != null)
                foreach (var size in filterRequest.Sizes)
                    query.Add("Sizes", size);

            if (filterRequest.Genders != null)
                foreach (var gender in filterRequest.Genders)
                    query.Add("Genders", gender);

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            var url = $"https://localhost:7257/api/TheThaoCustomer/BongRo/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageBongRo>(json);


            return View(pagedResult);
        }


        [HttpGet]
        public async Task<IActionResult> CauLongCustomer([FromQuery] CauLongFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
                foreach (var item in filterRequest.Product)
                    query.Add("Product", item);

            if (filterRequest.Colors != null)
                foreach (var color in filterRequest.Colors)
                    query.Add("Colors", color);

            if (filterRequest.Sizes != null)
                foreach (var size in filterRequest.Sizes)
                    query.Add("Sizes", size);

            if (filterRequest.Genders != null)
                foreach (var gender in filterRequest.Genders)
                    query.Add("Genders", gender);

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            var url = $"https://localhost:7257/api/TheThaoCustomer/CauLong/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageCauLong>(json);


            return View(pagedResult);
        }


        [HttpGet]
        public async Task<IActionResult> BongDaCustomer([FromQuery] BongDaFilterRequest filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy = string.IsNullOrWhiteSpace(filterRequest.SortBy) ? "createdat" : filterRequest.SortBy;
            filterRequest.SortOrder = string.IsNullOrWhiteSpace(filterRequest.SortOrder) ? "desc" : filterRequest.SortOrder;

            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
                foreach (var item in filterRequest.Product)
                    query.Add("Product", item);

            if (filterRequest.Colors != null)
                foreach (var color in filterRequest.Colors)
                    query.Add("Colors", color);

            if (filterRequest.Sizes != null)
                foreach (var size in filterRequest.Sizes)
                    query.Add("Sizes", size);

            if (filterRequest.Genders != null)
                foreach (var gender in filterRequest.Genders)
                    query.Add("Genders", gender);

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            var url = $"https://localhost:7257/api/TheThaoCustomer/BongDa/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageBongDa>(json);


            return View(pagedResult);
        }


        [HttpGet]
        public async Task<IActionResult> GolfCustomer([FromQuery] Golf filterRequest)
        {
            filterRequest.Page = filterRequest.Page <= 0 ? 1 : filterRequest.Page;
            filterRequest.PageSize = filterRequest.PageSize <= 0 ? 12 : filterRequest.PageSize;
            filterRequest.SortBy ??= "createdat";
            filterRequest.SortOrder ??= "desc";

            var query = HttpUtility.ParseQueryString(string.Empty);

            if (filterRequest.Product != null)
                foreach (var item in filterRequest.Product)
                    query.Add("Product", item);

            if (filterRequest.Colors != null)
                foreach (var color in filterRequest.Colors)
                    query.Add("Colors", color);

            if (filterRequest.Sizes != null)
                foreach (var size in filterRequest.Sizes)
                    query.Add("Sizes", size);

            if (filterRequest.Genders != null)
                foreach (var gender in filterRequest.Genders)
                    query.Add("Genders", gender);

            query.Add("SortBy", filterRequest.SortBy);
            query.Add("SortOrder", filterRequest.SortOrder);
            query.Add("Page", filterRequest.Page.ToString());
            query.Add("PageSize", filterRequest.PageSize.ToString());

            var url = $"https://localhost:7257/api/TheThaoCustomer/Golf/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageGolf>(json);


            return View(pagedResult);
        }


        // GET: TheThaoCustomer/Details/5
        //[HttpGet]
        //public async Task<IActionResult> DetailTheThaoCustomer(Guid id)
        //{
        //    var response = await _httpClient.GetAsync($"https://localhost:7257/api/TheThaoCustomer/{id}");
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        return NotFound();
        //    }
        //    var json = await response.Content.ReadAsStringAsync();
        //    var productDetail = JsonConvert.DeserializeObject<ProductDetailCustomerDto>(json);
        //    if (productDetail == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(productDetail);
        //    //return View();
        //}

        // GET: TheThaoCustomer/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TheThaoCustomer/Create
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

        // GET: TheThaoCustomer/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TheThaoCustomer/Edit/5
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

        // GET: TheThaoCustomer/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TheThaoCustomer/Delete/5
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
