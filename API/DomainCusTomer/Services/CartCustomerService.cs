using API.DomainCusTomer.DTOs.CartICustomer;
using API.DomainCusTomer.DTOs.MuangayCustomer;
using API.DomainCusTomer.Request.Cast;
using API.DomainCusTomer.Request.MuaNgay;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Policy;
using System.Text;

namespace API.DomainCusTomer.Services
{
    public class CartCustomerService : ICartCustomerService
    {
        private const string CookieCartKey = "CustomerCart";
        private const string SessionBuyNowKey = "CustomerBuyNow";
        private readonly DbContextApp _context;
        private const int MaxCookieSize = 3800; // 4KB limit

        public CartCustomerService(DbContextApp context)
        {
            _context = context;
        }
        //public async Task<List<CartCustomerDto>> GetCurrentAsync(HttpContext ctx)
        //{
        //    var cart = GetCartFromCookie(ctx) ?? new List<CartCustomerDto>();

        //    if (!cart.Any())
        //        return cart;

        //    var productCodes = cart.Select(x => x.ProductDetailcode).ToList();

        //    var products = await _context.ProductDetails
        //        .Where(p => productCodes.Contains(p.Code))
        //        .ToListAsync();

        //    foreach (var item in cart)
        //    {
        //        var product = products.FirstOrDefault(p => p.Code == item.ProductDetailcode);

        //        if (product == null || product.Quantity == 0)
        //        {

        //            continue;
        //        }


        //        if (item.Quantity > product.Quantity)
        //        {
        //            item.Quantity = 1;
        //            item.Price = product.Price; 
        //        }


        //        item.Amount = item.Quantity * product.Price;
        //    }

        //    SaveCartToCookie(ctx, cart);
        //    return cart;
        //}
        //public async Task<List<CartCustomerDto>> GetCurrentAsync(HttpContext ctx)
        //{
        //    var cart = GetCartFromCookie(ctx) ?? new List<CartCustomerDto>();

        //    if (!cart.Any())
        //        return cart;

        //    var productCodes = cart.Select(x => x.ProductDetailcode).ToList();

        //    var products = await _context.ProductDetails
        //        .Include(x => x.PromotionProducts)
        //            .ThenInclude(p => p.Promotion)
        //        .Where(p => productCodes.Contains(p.Code))
        //        .ToListAsync();

        //    foreach (var item in cart)
        //    {
        //        var product = products.FirstOrDefault(p => p.Code == item.ProductDetailcode);

        //        if (product == null || product.Quantity == 0)
        //        {
        //            continue;
        //        }

        //        // Áp dụng giá khuyến mãi nếu có
        //        decimal priceToUse = product.Price;
        //        var currentPromotion = product.PromotionProducts
        //            .FirstOrDefault(p => p.Promotion.Status == "1");

        //        if (currentPromotion != null && currentPromotion.Priceafterduction > 0)
        //        {
        //            priceToUse = currentPromotion.Priceafterduction;
        //        }

        //        // Nếu vượt quá tồn kho → điều chỉnh lại
        //        if (item.Quantity > product.Quantity)
        //        {
        //             item.Quantity = 1;
        //            item.Price = product.Price;
        //            item.Amount = product.Price * 1;
        //        }
        //        if (product.Quantity == 0)
        //        {
        //            item.Quantity = 0;
        //            item.Amount = 0;
        //        }
        //        else
        //        {
        //            item.Price = priceToUse;
        //            item.Amount = item.Quantity * priceToUse;
        //        }
        //    }

        //    SaveCartToCookie(ctx, cart);
        //    return cart;
        //}


        public async Task<List<CartCustomerDto>> GetCurrentAsync(HttpContext ctx)
        {
            var cart = GetCartFromCookie(ctx) ?? new List<CartCustomerDto>();

            if (!cart.Any())
                return cart;

            var productCodes = cart.Select(x => x.ProductDetailcode).ToList();

            var products = await _context.ProductDetails
                .Include(x => x.PromotionProducts)
                    .ThenInclude(p => p.Promotion)
                .Where(p => productCodes.Contains(p.Code))
                .ToListAsync();

            var updatedCart = new List<CartCustomerDto>();

            foreach (var item in cart)
            {
                var product = products.FirstOrDefault(p => p.Code == item.ProductDetailcode);

                // Nếu sản phẩm không tồn tại hoặc hết hàng → bỏ qua
                if (product == null || product.Quantity == 0)
                {
                    continue;
                }

                // Áp dụng giá khuyến mãi nếu có
                decimal priceToUse = product.Price;
                var currentPromotion = product.PromotionProducts
                    .FirstOrDefault(p => p.Promotion.Status == VoucherStatus.Active);

                if (currentPromotion != null && currentPromotion.Priceafterduction >= 0)
                {
                    priceToUse = currentPromotion.Priceafterduction;
                }

                // Nếu vượt quá tồn kho → điều chỉnh lại
                if (item.Quantity > product.Quantity)
                {
                    item.Quantity = 1;
                }
                //item.StockQuantity = product.Quantity;
                item.Price = priceToUse;
                item.Amount = item.Quantity * priceToUse;

                updatedCart.Add(item);
            }

            SaveCartToCookie(ctx, updatedCart);
            return updatedCart;
        }

        // ======= 2. Thêm sản phẩm =======
        public async Task<List<CartCustomerDto>> AddAsync(HttpContext ctx, CartCustomerRequest cartCustomerRequest)
        {
            if (cartCustomerRequest == null || string.IsNullOrEmpty(cartCustomerRequest.ProductDetailcode))
                throw new ArgumentException("Yêu cầu không hợp lệ");

            var findProduct = await _context.ProductDetails
                .Include(x => x.Color)
                .Include(x => x.Size)
                .Include(x => x.Images)
                .Include(p => p.PromotionProducts).ThenInclude(p => p.Promotion)
                .FirstOrDefaultAsync(x => x.Code == cartCustomerRequest.ProductDetailcode);

            if (findProduct == null)
                throw new Exception($"Sản phẩm {cartCustomerRequest.ProductDetailcode} không tồn tại");

            var cart = GetCartFromCookie(ctx) ?? new List<CartCustomerDto>();
            var existingItem = cart.FirstOrDefault(x => x.ProductDetailcode == findProduct.Code);

            decimal priceToUse = findProduct.Price;
            var currentPromotion = findProduct.PromotionProducts
                .FirstOrDefault(p => p.Promotion.Status == VoucherStatus.Active);
            if (currentPromotion != null && currentPromotion.Priceafterduction >= 0)
                priceToUse = currentPromotion.Priceafterduction;

            int addQty = cartCustomerRequest.Quantity > 0 ? cartCustomerRequest.Quantity : 1;

            // Nếu sản phẩm đã có trong giỏ, kiểm tra tổng mới không vượt quá tồn kho
            int totalRequestedQty = (existingItem?.Quantity ?? 0) + addQty;
            if (findProduct.Quantity < totalRequestedQty)
                throw new Exception($"Sản phẩm {findProduct.Name} chỉ còn {findProduct.Quantity} sản phẩm trong kho.");

            if (existingItem != null)
            {
                existingItem.Quantity += addQty;
                existingItem.Price = priceToUse;
                existingItem.Amount = existingItem.Quantity * priceToUse;
            }
            else
            {
                cart.Add(new CartCustomerDto
                {
                    ProductDetailId = findProduct.Id,
                    ProductDetailcode = findProduct.Code,
                    Name = findProduct.Name,
                    Quantity = addQty,
                    Price = priceToUse,
                    Amount = priceToUse * addQty,
                    ImageUrl = findProduct.Images.FirstOrDefault()?.Url ?? "default.jpg",
                    ColorName = findProduct.Color?.Name ?? "NO_COLOR",
                    ColorCode = findProduct.Color?.Code ?? "#000",
                    SizeName = findProduct.Size?.Name ?? "NO_SIZE"
                });
            }

            SaveCartToCookie(ctx, cart);
            return cart;
        }
        //// ======= 1. Lấy giỏ hàng =======
        //public async Task<List<CartCustomerDto>> GetCurrentAsync(HttpContext ctx)
        //{
        //    return GetCartFromCookie(ctx) ?? new List<CartCustomerDto>();
        //}

        //// ======= 2. Thêm sản phẩm =======
        //public async Task<List<CartCustomerDto>> AddAsync(HttpContext ctx, CartCustomerRequest cartCustomerRequest)
        //{
        //    if (cartCustomerRequest == null || string.IsNullOrEmpty(cartCustomerRequest.ProductDetailcode))
        //        throw new ArgumentException("Yêu cầu không hợp lệ");

        //    var findProduct = await _context.ProductDetails
        //        .Include(x => x.Color)
        //        .Include(x => x.Size)
        //        .Include(x => x.Images)
        //        .Include(p => p.PromotionProducts).ThenInclude(p => p.Promotion)
        //        .FirstOrDefaultAsync(x => x.Code == cartCustomerRequest.ProductDetailcode);

        //    if (findProduct == null)
        //        throw new Exception($"Sản phẩm {cartCustomerRequest.ProductDetailcode} không tồn tại");

        //    var cart = GetCartFromCookie(ctx) ?? new List<CartCustomerDto>();
        //    var existingItem = cart.FirstOrDefault(x => x.ProductDetailcode == findProduct.Code);

        //    decimal priceToUse = findProduct.Price;
        //    var currentPromotion = findProduct.PromotionProducts
        //        .Where(p => p.Promotion.Status == "1")
        //        .Select(p => p.Priceafterduction)
        //        .FirstOrDefault();

        //    if (currentPromotion > 0)
        //        priceToUse = currentPromotion;

        //    int addQty = cartCustomerRequest.Quantity > 0 ? cartCustomerRequest.Quantity : 1;
        //    if (findProduct.Quantity < addQty)
        //        throw new Exception($"Sản phẩm {findProduct.Name} chỉ còn {findProduct.Quantity} sản phẩm trong kho.");

        //    if (existingItem != null)
        //    {
        //        existingItem.Quantity += addQty;
        //        existingItem.Price = priceToUse;
        //    }
        //    else
        //    {
        //        cart.Add(new CartCustomerDto
        //        {
        //            ProductDetailId = findProduct.Id,
        //            ProductDetailcode = findProduct.Code,
        //            Name = findProduct.Name,
        //            Quantity = addQty,
        //            Price = priceToUse,
        //            ImageUrl = findProduct.Images.FirstOrDefault()?.Url ?? "default.jpg",
        //            ColorName = findProduct.Color?.Name ?? "NO_COLOR",
        //            ColorCode = findProduct.Color?.Code ?? "#000",
        //            SizeName = findProduct.Size?.Name ?? "NO_SIZE"
        //        });
        //    }

        //    SaveCartToCookie(ctx, cart);
        //    return cart;
        //}

        // ======= 3. Cập nhật số lượng =======
        //public async Task UpdateQtyAsync(HttpContext ctx, CartCustomerRequest cartCustomerRequest)
        //{
        //    if (string.IsNullOrEmpty(cartCustomerRequest.ProductDetailcode))
        //        throw new ArgumentException("Mã sản phẩm không hợp lệ");

        //    var cart = GetCartFromCookie(ctx) ?? new List<CartCustomerDto>();
        //    var item = cart.FirstOrDefault(x => x.ProductDetailcode == cartCustomerRequest.ProductDetailcode);

        //    if (item == null)
        //        throw new Exception("Sản phẩm không tồn tại trong giỏ hàng");

        //    // Lấy lại thông tin sản phẩm để kiểm tra tồn kho
        //    var productDetail = await _context.ProductDetails
        //        .FirstOrDefaultAsync(x => x.Code == cartCustomerRequest.ProductDetailcode);

        //    if (productDetail == null)
        //        throw new Exception("Không tìm thấy thông tin sản phẩm");

        //    // Giới hạn số lượng
        //    int newQty = cartCustomerRequest.Quantity <= 0 ? 1 : cartCustomerRequest.Quantity;
        //    if (newQty > productDetail.Quantity)
        //        throw new Exception($"Chỉ còn {productDetail.Quantity} sản phẩm trong kho.");

        //    item.Quantity = newQty;
        //    item.Price = newQty * productDetail.Price;

        //    SaveCartToCookie(ctx, cart);
        //}


        public async Task<List<string>> ValidateCartQuantityAsync(HttpContext ctx)
        {
            var cart = GetCartFromCookie(ctx) ?? new List<CartCustomerDto>();
            var errors = new List<string>();

            if (!cart.Any())
                return errors;

            var productCodes = cart.Select(x => x.ProductDetailcode).ToList();

            var products = await _context.ProductDetails
                .Where(p => productCodes.Contains(p.Code))
                .ToListAsync();

            foreach (var item in cart)
            {
                var product = products.FirstOrDefault(p => p.Code == item.ProductDetailcode);

                if (product == null)
                {
                    errors.Add($"Sản phẩm có mã '{item.ProductDetailcode}' không tồn tại.");
                    continue;
                }

                if (product.Quantity == 0)
                {
                    errors.Add($"Sản phẩm '{item.Name}' đã hết hàng.");
                }
                else if (item.Quantity > product.Quantity)
                {
                    errors.Add($"Sản phẩm '{item.Name}' chỉ còn {product.Quantity} trong kho.");
                }
            }

            return errors;
        }
        // ======= 4. Xóa sản phẩm =======
        public async Task RemoveAsync(HttpContext ctx, string ProductDetailcode)
        {
            if (string.IsNullOrEmpty(ProductDetailcode))
                throw new ArgumentException("Mã sản phẩm không hợp lệ");

            var cart = GetCartFromCookie(ctx) ?? new List<CartCustomerDto>();
            var item = cart.FirstOrDefault(x => x.ProductDetailcode == ProductDetailcode);

            if (item != null)
            {
                cart.Remove(item);
                SaveCartToCookie(ctx, cart);
            }
        }
        //public async Task<List<CartCustomerDto>> Updateincrease(HttpContext ctx, string ProductDetailcode)
        //{
        //    if (string.IsNullOrEmpty(ProductDetailcode))
        //        throw new ArgumentException("Mã sản phẩm không hợp lệ");

        //    var cart = GetCartFromCookie(ctx) ?? new List<CartCustomerDto>();
        //    var item = cart.FirstOrDefault(x => x.ProductDetailcode == ProductDetailcode);

        //    if (item == null)
        //        throw new Exception("Sản phẩm không tồn tại trong giỏ hàng");

        //    var productDetail = await _context.ProductDetails
        //        .Include(p => p.PromotionProducts).ThenInclude(p => p.Promotion)
        //        .FirstOrDefaultAsync(x => x.Code == ProductDetailcode);

        //    if (productDetail == null)
        //        throw new Exception("Không tìm thấy thông tin sản phẩm");

        //    if (item.Quantity + 1 > productDetail.Quantity)
        //        throw new Exception($"Chỉ còn {productDetail.Quantity} sản phẩm trong kho.");

        //    // Áp dụng giá khuyến mãi nếu có
        //    decimal priceToUse = productDetail.Price;
        //    var promo = productDetail.PromotionProducts.FirstOrDefault(p => p.Promotion.Status == "1");
        //    if (promo != null && promo.Priceafterduction > 0)
        //        priceToUse = promo.Priceafterduction;

        //    item.Quantity += 1;
        //    item.Price = priceToUse;
        //    item.Amount = item.Quantity * priceToUse;

        //    SaveCartToCookie(ctx, cart);
        //    return cart;
        //}
        public async Task Updateincrease(HttpContext ctx, string ProductDetailcode)
        {
            if (string.IsNullOrEmpty(ProductDetailcode))
                throw new ArgumentException("Mã sản phẩm không hợp lệ");

            var cart = GetCartFromCookie(ctx) ?? new List<CartCustomerDto>();
            var item = cart.FirstOrDefault(x => x.ProductDetailcode == ProductDetailcode);

            if (item == null)
                throw new Exception("Sản phẩm không tồn tại trong giỏ hàng");

            var productDetail = await _context.ProductDetails
                .Include(p => p.PromotionProducts).ThenInclude(p => p.Promotion)
                .FirstOrDefaultAsync(x => x.Code == ProductDetailcode);

            if (productDetail == null)
                throw new Exception("Không tìm thấy thông tin sản phẩm");

            if (item.Quantity + 1 > productDetail.Quantity)
                throw new Exception($"Chỉ còn {productDetail.Quantity} sản phẩm trong kho.");

            // Áp dụng giá khuyến mãi nếu có
            decimal priceToUse = productDetail.Price;
            var promo = productDetail.PromotionProducts.FirstOrDefault(p => p.Promotion.Status == VoucherStatus.Active);
            if (promo != null && promo.Priceafterduction > 0)
                priceToUse = promo.Priceafterduction;

            item.Quantity += 1;
            item.Price = priceToUse;
            item.Amount = item.Quantity * priceToUse;

            SaveCartToCookie(ctx, cart);


        }

        public async Task Updatereduce(HttpContext ctx, string ProductDetailcode)
        {
            if (string.IsNullOrEmpty(ProductDetailcode))
                throw new ArgumentException("Mã sản phẩm không hợp lệ");

            var cart = GetCartFromCookie(ctx) ?? new List<CartCustomerDto>();
            var item = cart.FirstOrDefault(x => x.ProductDetailcode == ProductDetailcode);
            var productDetail = await _context.ProductDetails
                .Include(p => p.PromotionProducts).ThenInclude(p => p.Promotion)
                .FirstOrDefaultAsync(x => x.Code == ProductDetailcode);
            if (item == null)
                throw new Exception("Sản phẩm không tồn tại trong giỏ hàng");

            if (item.Quantity <= 1)
                throw new Exception("Số lượng tối thiểu là 1");

            item.Quantity -= 1;
            item.Price = productDetail.Price;
            item.Amount = item.Quantity * item.Quantity;
            SaveCartToCookie(ctx, cart);
        }

        // ======= 5. Helpers =======
        private List<CartCustomerDto> GetCartFromCookie(HttpContext ctx)
        {
            if (ctx.Request.Cookies.TryGetValue(CookieCartKey, out string cookieValue))
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<CartCustomerDto>>(cookieValue);
                }
                catch
                {
                    return new List<CartCustomerDto>();
                }
            }
            return new List<CartCustomerDto>();
        }

        private void SaveCartToCookie(HttpContext ctx, List<CartCustomerDto> cart)
        {
            string json = JsonConvert.SerializeObject(cart);
            int size = Encoding.UTF8.GetByteCount(json);
            if (size > MaxCookieSize)
                throw new Exception("Giỏ hàng vượt quá dung lượng cookie cho phép (4KB). Vui lòng giảm số lượng sản phẩm.");
            ctx.Response.Cookies.Delete(CookieCartKey);
            ctx.Response.Cookies.Append(CookieCartKey, json);
        }
        public void SaveCartForProgram(HttpContext ctx, List<CartCustomerDto> cart)
        {
            SaveCartToCookie(ctx, cart);
        }


        // ================== 6. Mua Ngay (Session) ==================
        public async Task<MuangaycustomerDto> MuaNgayAddAsync(HttpContext ctx, MuaNgayCustomerRequest muaNgayCustomerRequest)
        {
            if (muaNgayCustomerRequest == null || string.IsNullOrEmpty(muaNgayCustomerRequest.ProductDetailcodeMuaNgay))
                throw new ArgumentException("Yêu cầu không hợp lệ");

            var findProduct = await _context.ProductDetails
                .Include(x => x.Color)
                .Include(x => x.Size)
                .Include(x => x.Images)
                .Include(p => p.PromotionProducts).ThenInclude(p => p.Promotion)
                .FirstOrDefaultAsync(x => x.Code == muaNgayCustomerRequest.ProductDetailcodeMuaNgay);

            if (findProduct == null)
                throw new Exception($"Sản phẩm {muaNgayCustomerRequest.ProductDetailcodeMuaNgay} không tồn tại");
            if (findProduct.Quantity < 1)
                throw new Exception("Sản phẩm hiện đang hết hàng.");
            //p.Promotion.StartDate <= DateTime.Now &&
            //                p.Promotion.EndDate >= DateTime.Now &&
            decimal priceToUse = findProduct.Price;
            //var currentPromotion = findProduct.PromotionProducts
            //    .Where(p =>  p.Promotion.Status == VoucherStatus.Active)
            //    .Select(p => p.Priceafterduction)
            //    .FirstOrDefault();
            //if (currentPromotion >= 0)
            //    priceToUse = currentPromotion;
            var currentPromotion = findProduct.PromotionProducts
              .FirstOrDefault(p => p.Promotion.Status == VoucherStatus.Active);
            if (currentPromotion != null && currentPromotion.Priceafterduction >= 0)
                priceToUse = currentPromotion.Priceafterduction;

            int addQty = muaNgayCustomerRequest.QuantityMuaNgay > 0 ? muaNgayCustomerRequest.QuantityMuaNgay : 1;
            if (addQty > findProduct.Quantity)
                throw new Exception($"Số lượng yêu cầu ({addQty}) vượt quá tồn kho ({findProduct.Quantity}).");
            var buyNowItem = new MuangaycustomerDto
            {
                Id = Guid.NewGuid(),
                ProductDetailId = findProduct.Id,
                ProductDetailcode = findProduct.Code,
                Name = findProduct.Name,
                Quantity = addQty,
                Price = priceToUse,
                Total = priceToUse * addQty,
                ImageUrl = findProduct.Images.FirstOrDefault()?.Url ?? "default.jpg",
                ColorName = findProduct.Color?.Name ?? "NO_COLOR",
                ColorCode = findProduct.Color?.Code ?? "#000",
                SizeName = findProduct.Size?.Name ?? "NO_SIZE"
            };

            ctx.Session.SetString(SessionBuyNowKey, JsonConvert.SerializeObject(buyNowItem));
            return buyNowItem;
        }

        public Task<MuangaycustomerDto> MuaNgayAsync(HttpContext ctx)
        {
            var json = ctx.Session.GetString(SessionBuyNowKey);
            return Task.FromResult(string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject<MuangaycustomerDto>(json));
        }
    }
}
