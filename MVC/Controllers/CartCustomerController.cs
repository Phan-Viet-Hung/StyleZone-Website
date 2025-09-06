using API.DomainCusTomer.DTOs.CartICustomer;
using API.DomainCusTomer.Request.Cast;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace MVC.Controllers
{
    public class CartCustomerController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string CookieCartKey = "CustomerCart";

        public CartCustomerController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7257/api/");
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
        [HttpGet]
        public IActionResult HeaderPartial()
        {
            return PartialView("_Hearder");
        }


        //[HttpGet]
        //public async Task<IActionResult> CartCustomerIndex()
        //{
        //    const string CookieCartKey = "CustomerCart";

        //    // 1. Gọi API lấy giỏ hàng hiện tại đã được xử lý tồn kho
        //    var response = await _httpClient.GetAsync("CartCustomer/current");
        //    if (!response.IsSuccessStatusCode)
        //        return View(new List<CartCustomerDto>());
        //    var updatedItemsJson = await response.Content.ReadAsStringAsync();
        //    var json = await response.Content.ReadAsStringAsync();
        //    var apiCart = JsonConvert.DeserializeObject<List<CartCustomerDto>>(json) ?? new();


        //    List<CartCustomerDto> existingCart = new();
        //    if (Request.Cookies.TryGetValue(CookieCartKey, out var cookieJson))
        //    {
        //        try
        //        {
        //            existingCart = JsonConvert.DeserializeObject<List<CartCustomerDto>>(cookieJson) ?? new();
        //        }
        //        catch
        //        {
        //            existingCart = new();
        //        }
        //    }


        //    foreach (var item in apiCart)
        //    {
        //        var existing = existingCart.FirstOrDefault(x => x.ProductDetailcode == item.ProductDetailcode);
        //        if (existing != null)
        //        {
        //            existing.Quantity = item.Quantity;
        //            existing.Price = item.Price;
        //            existing.Amount = existing.Price * existing.Quantity;
        //        }
        //        else
        //        {
        //            existingCart.Add(item);
        //        }
        //    }

        //    // 4. Cập nhật lại cookie
        //    var newJson = JsonConvert.SerializeObject(existingCart);
        //    Response.Cookies.Append(CookieCartKey, newJson, new CookieOptions
        //    {
        //        Expires = DateTimeOffset.Now.AddDays(7),
        //        HttpOnly = false,
        //        Secure = Request.IsHttps,
        //        SameSite = SameSiteMode.Lax,
        //        Path = "/"
        //    });

        //    return View(existingCart);
        //}
        [HttpGet]
        public async Task<IActionResult> CartCustomerIndex()
        {
            //const string CookieCartKey = "CustomerCart";
          string  username = HttpContext.Request.Cookies["UserName"] ?? HttpContext.Request.Cookies["LoginMethod"];

            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "Home");

            // B1: Gọi API lấy giỏ hàng hợp lệ từ server
            var response = await _httpClient.GetAsync("CartCustomer/current");
            if (!response.IsSuccessStatusCode)
            {
                return View(new List<CartCustomerDto>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiCart = JsonConvert.DeserializeObject<List<CartCustomerDto>>(json) ?? new();

            // B2: Lấy giỏ hàng cũ từ cookie
            var existingCart = new List<CartCustomerDto>();
            if (Request.Cookies.TryGetValue(CookieCartKey, out var cookieJson))
            {
                try
                {
                    existingCart = JsonConvert.DeserializeObject<List<CartCustomerDto>>(cookieJson) ?? new();
                }
                catch
                {
                    existingCart = new();
                }
            }

            var updatedCart = new List<CartCustomerDto>();

            // B3: Đồng bộ dữ liệu từ API với cookie
            foreach (var item in apiCart)
            {
                if (item.Quantity == 0 )
                    continue;

                var existing = existingCart.FirstOrDefault(x => x.ProductDetailcode == item.ProductDetailcode);

                if (existing != null)
                {
                    existing.Quantity = item.Quantity;
                    existing.Price = item.Price;
                    existing.Amount = existing.Price * existing.Quantity;
                }
                else
                        {
                    // Thêm sản phẩm mới, giữ nguyên số lượng từ item
                    existingCart.Add(item);
                }
            }

            // B4: Lưu cookie mới
            var newJson = JsonConvert.SerializeObject(existingCart);
            Response.Cookies.Append(CookieCartKey, newJson, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                HttpOnly = false,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            // B5: Trả về View
            return View(existingCart);
        }




        [HttpPost]
        public async Task<IActionResult> AddToCartCustomer(CartCustomerRequest request)
        {
            const string CookieCartKey = "CustomerCart";

            // 1. Kiểm tra giỏ hàng trong cookie
            if (Request.Cookies.TryGetValue(CookieCartKey, out var cookieJson))
            {
                try
                {
                    var cart = JsonConvert.DeserializeObject<List<CartCustomerDto>>(cookieJson);
                    if (cart == null || !cart.Any())
                    {
                        // Nếu giỏ rỗng => xóa cookie và gọi API clear server cart
                        Response.Cookies.Delete(CookieCartKey);

                    }
                }
                catch
                {
                    // Nếu lỗi khi parse => vẫn xóa cookie và clear API để tránh lỗi đồng bộ
                    Response.Cookies.Delete(CookieCartKey);

                }
            }

            // 2. Gửi yêu cầu thêm sản phẩm mới
            var jsonReq = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("CartCustomer/add", content);
            if (!response.IsSuccessStatusCode)
                return BadRequest("Không thể thêm sản phẩm vào giỏ.");

            // 3. Lấy giỏ hàng hiện tại từ cookie
            List<CartCustomerDto> existingCart = new();
            if (Request.Cookies.TryGetValue(CookieCartKey, out var cookieCartJson))
            {
                try
                {
                    existingCart = JsonConvert.DeserializeObject<List<CartCustomerDto>>(cookieCartJson) ?? new();
                }
                catch
                {
                    existingCart = new();
                }
            }

            // 4. Lấy giỏ hàng từ API sau khi thêm
            var updatedItemsJson = await response.Content.ReadAsStringAsync();
            var updatedItems = JsonConvert.DeserializeObject<List<CartCustomerDto>>(updatedItemsJson) ?? new();

            // 5. Merge sản phẩm mới
            foreach (var item in updatedItems)
            {
                var existing = existingCart.FirstOrDefault(x => x.ProductDetailcode == item.ProductDetailcode);
                if (existing != null)
                {
                    existing.Quantity = item.Quantity;
                    existing.Price = item.Price;
                    existing.Amount = item.Price * item.Quantity;
                }
                else
                {
                    existingCart.Add(item);
                }
            }

            // 6. Ghi lại cookie
            var newJson = JsonConvert.SerializeObject(existingCart);
            Response.Cookies.Append(CookieCartKey, newJson, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                HttpOnly = false,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            return RedirectToAction("CartCustomerIndex");
        }

     
     

        [HttpGet]
        public async Task<IActionResult> RemoveFromCartCustomer(string ProductDetailcode)
        {
            const string CookieCartKey = "CustomerCart";
            var response = await _httpClient.GetAsync($"CartCustomer/Remove?ProductDetailcode={ProductDetailcode}");
            if (!response.IsSuccessStatusCode)
                return BadRequest("Không thể xóa sản phẩm.");
            var updatedItemsJson = await response.Content.ReadAsStringAsync();

            List<CartCustomerDto> updatedItems;
            try
            {
                updatedItems = JsonConvert.DeserializeObject<List<CartCustomerDto>>(updatedItemsJson) ?? new();
            }
            catch
            {
                updatedItems = new List<CartCustomerDto>();
            }

            // Lấy giỏ hàng hiện tại từ cookie
            List<CartCustomerDto> existingCart = new();
            if (Request.Cookies.TryGetValue(CookieCartKey, out var cookieCartJson))
            {
                try
                {
                    existingCart = JsonConvert.DeserializeObject<List<CartCustomerDto>>(cookieCartJson) ?? new();
                }
                catch
                {
                    existingCart = new();
                }
            }
            existingCart = existingCart.Where(x => x.ProductDetailcode != ProductDetailcode).ToList();
            // Ghi lại cookie mới
            var newJson = JsonConvert.SerializeObject(existingCart);
            Response.Cookies.Append(CookieCartKey, newJson, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                HttpOnly = false,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });
            return RedirectToAction(nameof(CartCustomerIndex));
        }

        [HttpGet]
        public async Task<IActionResult> IncreaseFromCartCustomer(string ProductDetailcode)
        {
            const string CookieCartKey = "CustomerCart";

            var response = await _httpClient.GetAsync($"CartCustomer/increase?ProductDetailcode={ProductDetailcode}");
            var updatedItemsJson = await response.Content.ReadAsStringAsync();
            

            List<CartCustomerDto> updatedItems;
            try
            {
                updatedItems = JsonConvert.DeserializeObject<List<CartCustomerDto>>(updatedItemsJson) ?? new();
            }
            catch
            {
                updatedItems = new List<CartCustomerDto>();
            }
            List<CartCustomerDto> existingCart = new();

            if (Request.Cookies.TryGetValue(CookieCartKey, out var cookieCartJson))
            {
                try
                {
                    existingCart = JsonConvert.DeserializeObject<List<CartCustomerDto>>(cookieCartJson) ?? new();
                }
                catch
                {
                    existingCart = new();
                }
            }

            if (updatedItems.Any())
            {
                foreach (var item in updatedItems)
                {
                    var existing = existingCart.FirstOrDefault(x => x.ProductDetailcode == item.ProductDetailcode);
                    if (existing != null)
                    {
                        existing.Quantity = item.Quantity;
                        existing.Price = item.Price;
                        existing.Amount = item.Price * item.Quantity;
                    }
                    else
                    {
                        existingCart.Add(item);
                    }
                }
            }
            else
            {
                var item = existingCart.FirstOrDefault(x => x.ProductDetailcode == ProductDetailcode);
                if (item != null)
                {
                    item.Quantity += 1;
                }
            }

            var newJson = JsonConvert.SerializeObject(existingCart);
            Response.Cookies.Append(CookieCartKey, newJson, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                HttpOnly = false,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            return RedirectToAction(nameof(CartCustomerIndex));
        }



        [HttpGet]
        public async Task<IActionResult> ReduceFromCartCustomer(string ProductDetailcode)
        {
            const string CookieCartKey = "CustomerCart";

            var response = await _httpClient.GetAsync($"CartCustomer/reduce?ProductDetailcode={ProductDetailcode}");



            var updatedItemsJson = await response.Content.ReadAsStringAsync();

            List<CartCustomerDto> updatedItems;
            try
            {
                updatedItems = JsonConvert.DeserializeObject<List<CartCustomerDto>>(updatedItemsJson) ?? new();
            }
            catch
            {
                updatedItems = new List<CartCustomerDto>();
            }

            List<CartCustomerDto> existingCart = new();
            if (Request.Cookies.TryGetValue(CookieCartKey, out var cookieCartJson))
            {
                try
                {
                    existingCart = JsonConvert.DeserializeObject<List<CartCustomerDto>>(cookieCartJson) ?? new();
                }
                catch
                {
                    existingCart = new();
                }
            }

            if (updatedItems.Any())
            {
                foreach (var item in updatedItems)
                {
                    var existing = existingCart.FirstOrDefault(x => x.ProductDetailcode == item.ProductDetailcode);
                    if (existing != null)
                    {
                        existing.Quantity = item.Quantity;
                        existing.Price = item.Price;
                        existing.Amount = item.Price * item.Quantity;
                    }
                }
            }
            else
            {
                var item = existingCart.FirstOrDefault(x => x.ProductDetailcode == ProductDetailcode);
                if (item != null)
                {
                    item.Quantity -= 1;
                    if (item.Quantity <= 0)
                    {
                        existingCart.Remove(item);
                    }
                }
            }

            var newJson = JsonConvert.SerializeObject(existingCart);
            Response.Cookies.Append(CookieCartKey, newJson, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                HttpOnly = false,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            return RedirectToAction(nameof(CartCustomerIndex));
        }

        [HttpGet]
        public async Task<IActionResult> CartBeforeCheckout()
        {
            return RedirectToAction("IndexThanhToan", "ThanhToanCustomer");
        }
    }
}
