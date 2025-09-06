using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;
using MVC.Models.Cart;
using Newtonsoft.Json;
using System.Text;

namespace MVC.Services
{
    public class CartCustomerMVCService : ICartCustomerMVCService
    {
        private const string CookieCartKey = "CustomerCart";
        private const int MaxCookieSize = 3800;
        private readonly DbContextApp _context;

        public CartCustomerMVCService(DbContextApp context)
        {
            _context = context;
        }
        public Task<List<CartCustomerMVCDto>> GetCurrentAsync(HttpContext ctx)
        {
            return Task.FromResult(GetCartFromCookie(ctx) ?? new List<CartCustomerMVCDto>());
        }
        public async Task<List<CartCustomerMVCDto>> AddAsync(HttpContext ctx, CartCustomerMVCRequest cartCustomerRequest)
        {
            if (cartCustomerRequest == null || string.IsNullOrEmpty(cartCustomerRequest.ProductDetailcode))
                throw new ArgumentException("Yêu cầu không hợp lệ");

            // Tìm sản phẩm
            var findProduct = await _context.ProductDetails
                .Include(x => x.Color)
                .Include(x => x.Size)
                .Include(x => x.Images)
                .Include(p => p.PromotionProducts).ThenInclude(p => p.Promotion)
                .FirstOrDefaultAsync(x => x.Code == cartCustomerRequest.ProductDetailcode);

            if (findProduct == null)
                throw new Exception($"Sản phẩm {cartCustomerRequest.ProductDetailcode} không tồn tại");

            // Lấy giỏ hàng từ cookie
            var cart = GetCartFromCookie(ctx) ?? new List<CartCustomerMVCDto>();
            var existingItem = cart.FirstOrDefault(x => x.ProductDetailcode == findProduct.Code);

            // Tính giá
            decimal priceToUse = findProduct.Price;
            var currentPromotion = findProduct.PromotionProducts
                .Where(p => p.Promotion.Status == VoucherStatus.Active)
                .Select(p => p.Priceafterduction)
                .FirstOrDefault();

            if (currentPromotion > 0)
                priceToUse = currentPromotion;

            int addQty = cartCustomerRequest.Quantity > 0 ? cartCustomerRequest.Quantity : 1;
            if (findProduct.Quantity < addQty)
                throw new Exception($"Sản phẩm {findProduct.Name} chỉ còn {findProduct.Quantity} sản phẩm trong kho.");

            // Cập nhật hoặc thêm sản phẩm
            if (existingItem != null)
            {
                existingItem.Quantity += addQty;
                existingItem.Price = priceToUse;
            }
            else
            {
                cart.Add(new CartCustomerMVCDto
                {
                    ProductDetailcode = findProduct.Code,
                    Name = findProduct.Name,
                    Quantity = addQty,
                    Price = priceToUse,
                    ImageUrl = findProduct.Images.FirstOrDefault()?.Url ?? "default.jpg",
                    ColorName = findProduct.Color?.Name ?? "NO_COLOR",
                    ColorCode = findProduct.Color?.Code ?? "#000",
                    SizeName = findProduct.Size?.Name ?? "NO_SIZE"
                });
            }
            SaveCartToCookie(ctx, cart);
            return cart;
        }

     

        public Task RemoveAsync(HttpContext ctx, string productCode)
        {
            var cart = GetCartFromCookie(ctx) ?? new List<CartCustomerMVCDto>();
            var item = cart.FirstOrDefault(x => x.ProductDetailcode == productCode);
            if (item != null)
            {
                cart.Remove(item);
                SaveCartToCookie(ctx, cart);
            }
            return Task.CompletedTask;
        }

        public Task UpdateQtyAsync(HttpContext ctx, CartCustomerMVCRequest cartCustomerRequest)
        {
            var cart = GetCartFromCookie(ctx) ?? new List<CartCustomerMVCDto>();
            var item = cart.FirstOrDefault(x => x.ProductDetailcode == cartCustomerRequest.ProductDetailcode);
            if (item != null)
            {
                item.Quantity = cartCustomerRequest.Quantity > 0 ? cartCustomerRequest.Quantity : item.Quantity;
                SaveCartToCookie(ctx, cart);
            }
            return Task.CompletedTask;
        }

        private List<CartCustomerMVCDto> GetCartFromCookie(HttpContext ctx)
        {
            if (ctx.Request.Cookies.TryGetValue(CookieCartKey, out string cookieValue))
                return JsonConvert.DeserializeObject<List<CartCustomerMVCDto>>(cookieValue);
            return new List<CartCustomerMVCDto>();
        }

        private void SaveCartToCookie(HttpContext ctx, List<CartCustomerMVCDto> cart)
        {
            string json = JsonConvert.SerializeObject(cart);
            int size = Encoding.UTF8.GetByteCount(json);
            if (size > MaxCookieSize)
                throw new Exception("Giỏ hàng quá lớn để lưu trong cookie (vượt quá 4KB).");

            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7), // Lưu 7 ngày
                HttpOnly = false,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };
            ctx.Response.Cookies.Append(CookieCartKey, json, options);
        }

        public void SaveCartForProgram(HttpContext ctx, List<CartCustomerMVCDto> cart)
        {
            SaveCartToCookie(ctx, cart);
        }
    }
}
