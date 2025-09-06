using API.DomainCusTomer.DTOs;
using API.DomainCusTomer.DTOs.ThoiTrang;
using API.DomainCusTomer.Request;
using API.DomainCusTomer.Request.ThoiTrang;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Web;

namespace MVC.Controllers
{
    public class ThoiTrangCustomerMVCController : Controller
    {
        private readonly HttpClient _httpClient;

        //private readonly string 
        public ThoiTrangCustomerMVCController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: TheThaoCustomer
        [HttpGet]
        public async Task<IActionResult> ThoiTrangCusTomer([FromQuery] ThoiTrangFilterRequst filterRequest)
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

            var url = $"https://localhost:7257/api/ThoiTrangCustomer/ThoiTrang/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageThoiTrang>(json);


            return View(pagedResult);
        }

        [HttpGet]
        public async Task<IActionResult> WADECusTomer([FromQuery] WadeFilterRequst filterRequest)
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

            var url = $"https://localhost:7257/api/ThoiTrangCustomer/WADE/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageWaDe>(json);


            return View(pagedResult);
        }
        [HttpGet]
        public async Task<IActionResult> BADFIVECusTomer([FromQuery] BADFIVEFilterRequst filterRequest)
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

            var url = $"https://localhost:7257/api/ThoiTrangCustomer/BADFIVE/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageBADFIVE>(json);


            return View(pagedResult);
        }
        [HttpGet]
        public async Task<IActionResult> LIFESTYLECusTomer([FromQuery] LIFESTYLEFilterRequst filterRequest)
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

            var url = $"https://localhost:7257/api/ThoiTrangCustomer/LIFESTYLE/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageLIFESTYLE>(json);


            return View(pagedResult);
        }
        [HttpGet]
        public async Task<IActionResult> ISAACCusTomer([FromQuery] ISAACFilterRequst filterRequest)
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

            var url = $"https://localhost:7257/api/ThoiTrangCustomer/ISAAC/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageISAAC>(json);


            return View(pagedResult);
        }
        [HttpGet]
        public async Task<IActionResult> YOUNGCusTomer([FromQuery] YOUNGFilterRequst filterRequest)
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

            var url = $"https://localhost:7257/api/ThoiTrangCustomer/YOUNG/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageYOUNG>(json);


            return View(pagedResult);
        }
        [HttpGet]
        public async Task<IActionResult> BeTraiCusTomer([FromQuery] BeTraiFilterRequst filterRequest)
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

            var url = $"https://localhost:7257/api/ThoiTrangCustomer/Betrai/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageBeTrai>(json);


            return View(pagedResult);
        }
        [HttpGet]
        public async Task<IActionResult> BeGaiCusTomer([FromQuery] BeGaiFilterRequst filterRequest)
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

            var url = $"https://localhost:7257/api/ThoiTrangCustomer/BeGai/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageBeGai>(json);


            return View(pagedResult);
        }
        [HttpGet]
        public async Task<IActionResult> OUTLETCusTomer([FromQuery] OUTLETFilterRequst filterRequest)
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

            var url = $"https://localhost:7257/api/ThoiTrangCustomer/OUTLET/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageOUTLET>(json);


            return View(pagedResult);
        }
        [HttpGet]
        public async Task<IActionResult> OUTLETPICKLEBALLCusTomer([FromQuery] OUTLETPICKLEBALLFilterRequst filterRequest)
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

            var url = $"https://localhost:7257/api/ThoiTrangCustomer/OUTLETPICKLEBALL/?{query}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"Lỗi khi gọi API: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var pagedResult = JsonConvert.DeserializeObject<PageOUTLETPICKLEBALL>(json);


            return View(pagedResult);
        }
        // GET: ThoiTrangCustomerMVCController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ThoiTrangCustomerMVCController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ThoiTrangCustomerMVCController/Create
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

        // GET: ThoiTrangCustomerMVCController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ThoiTrangCustomerMVCController/Edit/5
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

        // GET: ThoiTrangCustomerMVCController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ThoiTrangCustomerMVCController/Delete/5
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
