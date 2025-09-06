using API.DomainCusTomer.DTOs.CartICustomer;
using API.DomainCusTomer.DTOs.CastCustomerId;
using API.DomainCusTomer.DTOs.MuangayCustomer;
using API.DomainCusTomer.DTOs.MuaNgayCustomerID;
using API.DomainCusTomer.DTOs.ThanhToanCustomerId;
using API.DomainCusTomer.Request.Cast;
using API.DomainCusTomer.Request.MuaNgay;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace API.DomainCusTomer.Services
{
    public class CartCustomerIDServices : ICartCustomerIDServices
    {
        private readonly DbContextApp _context;
        private const string SessionBuyNowKey = "CustomerBuyNow";
        public CartCustomerIDServices(DbContextApp context)
        {
            _context = context;
        }
        public async Task<List<CastCustomerIDDto>> GetCurrenIDtAsync(string username)
        {
            var customerId = await _context.Customers
                .Where(c => c.UserName == username)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (customerId == Guid.Empty)
                return new List<CastCustomerIDDto>();

            var cartId = await _context.Carts
                .Where(c => c.CustomerId == customerId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (cartId == Guid.Empty)
                return new List<CastCustomerIDDto>();

            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .Include(ci => ci.ProductDetail)
                    .ThenInclude(pd => pd.Product)
                .Include(ci => ci.ProductDetail)
                    .ThenInclude(pd => pd.Color)
                .Include(ci => ci.ProductDetail)
                    .ThenInclude(pd => pd.Size)
                .Include(ci => ci.ProductDetail)
                    .ThenInclude(pd => pd.Images)
                .ToListAsync();

            // Map sang DTO
            var result = cartItems.Select(ci => new CastCustomerIDDto
            {
                Id = ci.Id,
                productdetailcode = ci.ProductDetail?.Code,
                Quantity = ci.Quantity,
                Price = ci.Price,
                ProductName = ci.ProductDetail?.Product?.Name,
                ColorName = ci.ProductDetail?.Color?.Name,
                SizeName = ci.ProductDetail?.Size?.Name,
                ImageUrl = ci.ProductDetail?.Images?.FirstOrDefault()?.Url ?? "/images/default.jpg",
                StockQuantity = ci.ProductDetail?.Quantity // ✅ Lấy số lượng tồn kho
            }).ToList();

            return result;
        }
        public async Task AddIDAsync(string username, CartCustomerRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ProductDetailcode))
                throw new ArgumentException("Yêu cầu không hợp lệ");

            var customer = await _context.Customers
                .Include(c => c.Cart)
                    .ThenInclude(cart => cart.CartItems)
                .FirstOrDefaultAsync(c => c.UserName == username);

            if (customer == null)
                throw new Exception("Không tìm thấy khách hàng");

            if (customer.Cart == null)
                throw new Exception("Giỏ hàng chưa được tạo cho khách hàng");

            var cart = customer.Cart;

            // Tìm thông tin sản phẩm
            var product = await _context.ProductDetails
                .Include(p => p.PromotionProducts)
                    .ThenInclude(p => p.Promotion)
                .FirstOrDefaultAsync(p => p.Code == request.ProductDetailcode);

            if (product == null)
                throw new Exception($"Sản phẩm {request.ProductDetailcode} không tồn tại");

            int qtyToAdd = request.Quantity > 0 ? request.Quantity : 1;

            var existingItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductDetailId == product.Id);

            // Tính tổng số lượng sau khi thêm
            int totalRequestedQty = (existingItem?.Quantity ?? 0) + qtyToAdd;
            if (product.Quantity < totalRequestedQty)
                throw new Exception($"Sản phẩm {product.Name} chỉ còn {product.Quantity} sản phẩm trong kho.");

            if (existingItem != null)
            {
                existingItem.Quantity += qtyToAdd;
            }
            else
            {
                decimal price = product.Price;
                var promotion = product.PromotionProducts
                    .FirstOrDefault(p => p.Promotion.Status == VoucherStatus.Active);

                if (promotion != null && promotion.Priceafterduction >= 0)
                    price = promotion.Priceafterduction;

                var newItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductDetailId = product.Id,
                    Quantity = qtyToAdd,
                    Price = price
                };

                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
        }


        public async Task AddListAsync(string username, List<CartCustomerRequest> requests)
        {
            var customer = await _context.Customers
                .Include(c => c.Cart)
                    .ThenInclude(cart => cart.CartItems)
                .FirstOrDefaultAsync(c => c.UserName == username);

            if (customer == null)
                throw new Exception("Không tìm thấy khách hàng");

            if (customer.Cart == null)
                throw new Exception("Giỏ hàng chưa được tạo cho khách hàng");

            var cart = customer.Cart;

            // Lấy danh sách code sản phẩm cần thêm
            var productCodes = requests.Select(r => r.ProductDetailcode).Distinct().ToList();

            // Lấy toàn bộ sản phẩm trong 1 lần query
            var products = await _context.ProductDetails
                .Include(p => p.PromotionProducts)
                    .ThenInclude(p => p.Promotion)
                .Where(p => productCodes.Contains(p.Code))
                .ToListAsync();

            foreach (var request in requests)
            {
                var product = products.FirstOrDefault(p => p.Code == request.ProductDetailcode);
                if (product == null)
                    continue; // bỏ qua nếu không tìm thấy

                var qty = request.Quantity > 0 ? request.Quantity : 1;

                // Kiểm tra sản phẩm đã tồn tại trong giỏ chưa
                var existingItem = cart.CartItems
                    .FirstOrDefault(ci => ci.ProductDetailId == product.Id);

                if (existingItem != null)
                {
                    existingItem.Quantity += qty;
                }
                else
                {
                    decimal price = product.Price;
                    var promotion = product.PromotionProducts
                        .FirstOrDefault(p => p.Promotion.Status == VoucherStatus.Active);

                    if (promotion != null && promotion.Priceafterduction >= 0)
                        price = promotion.Priceafterduction;

                    var newItem = new CartItem
                    {
                        Id = Guid.NewGuid(),
                        CartId = cart.Id,
                        ProductDetailId = product.Id,
                        Quantity = qty,
                        Price = price
                    };

                    _context.CartItems.Add(newItem);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveIDAsync(Guid Id)
        {
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(x => x.Id == Id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }


        public async Task UpdateIDIncreaseAsync(Guid Id)
        {
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(x => x.Id == Id);
            if (cartItem != null)
            {
                cartItem.Quantity += 1;
                await _context.SaveChangesAsync();
            }
        }


        public async Task UpdateIDReduceAsync(Guid Id)
        {
            var cartItem = await _context.CartItems.FirstOrDefaultAsync(x => x.Id == Id);
            if (cartItem != null && cartItem.Quantity > 1)
            {
                cartItem.Quantity -= 1;
                await _context.SaveChangesAsync();
            }
        }
        // ================== 6. Mua Ngay (Session) ==================
        //public async Task<MuangaycustomerIdDto> MuaNgayAddAsync(HttpContext ctx, MuaNgayCustomerRequest request, string username)
        //{
        //    if (request == null || string.IsNullOrWhiteSpace(request.ProductDetailcodeMuaNgay))
        //        throw new ArgumentException("Yêu cầu không hợp lệ");

        //    // Lấy customerId từ username
        //    var customerId = await _context.Customers
        //        .Where(c => c.UserName == username)
        //        .Select(c => c.Id)
        //        .FirstOrDefaultAsync();

        //    if (customerId == Guid.Empty)
        //        throw new Exception("Không tìm thấy khách hàng.");

        //    // Lấy thông tin sản phẩm
        //    var product = await _context.ProductDetails
        //        .Include(x => x.Color)
        //        .Include(x => x.Size)
        //        .Include(x => x.Images)
        //        .Include(p => p.PromotionProducts)
        //            .ThenInclude(pp => pp.Promotion)
        //        .FirstOrDefaultAsync(x => x.Code == request.ProductDetailcodeMuaNgay);

        //    if (product == null)
        //        throw new Exception($"Sản phẩm {request.ProductDetailcodeMuaNgay} không tồn tại");

        //    if (product.Quantity < 1)
        //        throw new Exception("Sản phẩm hiện đang hết hàng.");

        //    // Xác định giá áp dụng
        //    decimal priceToUse = product.Price;
        //    var currentPromotion = product.PromotionProducts
        //        .Where(pp => pp.p.Promotion.Status == VoucherStatus.Active)
        //        .Select(pp => pp.Priceafterduction)
        //        .FirstOrDefault();

        //    if (currentPromotion > 0)
        //        priceToUse = currentPromotion;

        //    // Số lượng mua
        //    int quantityToBuy = request.QuantityMuaNgay > 0 ? request.QuantityMuaNgay : 1;
        //    if (quantityToBuy > product.Quantity)
        //        throw new Exception($"Số lượng yêu cầu ({quantityToBuy}) vượt quá tồn kho ({product.Quantity}).");

        //    // Lấy địa chỉ
        //    var addresses = await _context.Addresses
        //        .Where(a => a.CustomerId == customerId)
        //        .Select(a => new AddressDto
        //        {
        //            Id = a.Id,
        //            FullName = a.FullName,
        //            PhoneNumber = a.PhoneNumber,
        //            DetailAddress = a.DetailAddress,
        //            Ward = a.Ward,
        //            District = a.District,
        //            Province = a.Province,
        //            Stastus = a.Status
        //        })
        //        .ToListAsync();

        //    // Lấy voucher khả dụng
        //    var vouchers = await _context.Vouchers
        //        .Where(v => v.Status == VoucherStatus.Active
        //                    && v.EndDate >= DateTime.Now
        //                    && _context.CustomerVouchers.Count(cv => cv.VoucherId == v.Id && cv.CustomerId == customerId) < v.MaxUsagePerCustomer
        //        )
        //        .Select(v => new VoucherDto
        //        {
        //            Id = v.Id,
        //            Code = v.Code,
        //            Description = v.Description,
        //            TotalUsageLimit = v.TotalUsageLimit,
        //            DiscountValue = v.DiscountValue,
        //            MinOrderAmount = v.MinOrderAmount,
        //            StartDate = v.StartDate,
        //            EndDate = v.EndDate,
        //            DiscountType = v.DiscountType,
        //            MaxUsagePerCustomer = v.MaxUsagePerCustomer
        //        })
        //        .ToListAsync();

        //    // Tạo DTO trả về
        //    var buyNowItem = new MuangaycustomerIdDto
        //    {
        //        Id = Guid.NewGuid(),
        //        ProductDetailId = product.Id,
        //        ProductDetailcode = product.Code,
        //        Name = product.Name,
        //        Quantity = quantityToBuy,
        //        Price = priceToUse,
        //        Total = priceToUse * quantityToBuy,
        //        ImageUrl = product.Images.FirstOrDefault()?.Url ?? "default.jpg",
        //        ColorName = product.Color?.Name ?? "NO_COLOR",
        //        ColorCode = product.Color?.Code ?? "#000",
        //        SizeName = product.Size?.Name ?? "NO_SIZE",
        //        AddressList = addresses,
        //        VoucherList = vouchers
        //    };

        //    // Lưu vào Session
        //    ctx.Session.SetString(SessionBuyNowKey, JsonConvert.SerializeObject(buyNowItem));

        //    return buyNowItem;
        //}
        //public Task<MuangaycustomerIdDto> MuaNgayAsync(HttpContext ctx)
        //{
        //    var json = ctx.Session.GetString(SessionBuyNowKey);
        //    if (string.IsNullOrEmpty(json))
        //        return Task.FromResult<MuangaycustomerIdDto>(null);

        //    return Task.FromResult(JsonConvert.DeserializeObject<MuangaycustomerIdDto>(json));
        //}

        public async Task<List<string>> ValidateIDCartQuantityAsync(string username)
        {
            var errors = new List<string>();

           
            var cart = await GetCurrenIDtAsync(username);

            if (!cart.Any())
                return errors;
            var productCodes = cart
                .Where(x => !string.IsNullOrEmpty(x.productdetailcode))
                .Select(x => x.productdetailcode)
                .ToList();
            var products = await _context.ProductDetails
                .Where(p => productCodes.Contains(p.Code))
                .ToListAsync();

            foreach (var item in cart)
            {
                var product = products.FirstOrDefault(p => p.Code == item.productdetailcode);

                if (product == null)
                {
                    errors.Add($"Sản phẩm có mã '{item.productdetailcode}' không tồn tại.");
                    continue;
                }

                if (product.Quantity == 0)
                {
                    errors.Add($"Sản phẩm '{item.ProductName}' đã hết hàng.");
                }
                else if (item.Quantity > product.Quantity)
                {
                    errors.Add($"Sản phẩm '{item.ProductName}' chỉ còn {product.Quantity} trong kho.");
                }
            }

            return errors;
        }
    }

}
