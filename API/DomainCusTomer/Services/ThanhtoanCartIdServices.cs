using API.DomainCusTomer.DTOs.CastCustomerId;
using API.DomainCusTomer.DTOs.MuaNgayCustomerID;
using API.DomainCusTomer.DTOs.ThanhToanCustomer;
using API.DomainCusTomer.DTOs.ThanhToanCustomerId;
using API.DomainCusTomer.Request.MuaNgay;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace API.DomainCusTomer.Services
{
    public class ThanhtoanCartIdServices : IThanhtoanCartIdServices
    {
        private readonly DbContextApp _context;
        private const string SessionBuyNowKey = "CustomerBuyNow";
        public ThanhtoanCartIdServices(DbContextApp context)
        {
            _context = context;
        }
        public async Task<OrderID> CreateOrderAsyncCustomerid(OrderCustomerIdDto request, string username)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.UserName == username);
            if (request.PaymentMethodCode == "cod" && customer.Status != "Active")
                throw new InvalidOperationException("Khách hàng không được phép thực hiện đơn hàng COD.");

            var order = new OrderInfo
            {
                Id = Guid.NewGuid(),
                CustomerName = request.FullName,
                CustomerId = customer.Id,
                PhoneNumber = request.PhoneNumber,
                Address = $"{request.Address}, {request.Ward}, {request.District}, {request.Province}",
                ShippingFee = request.ShippingFee,
                TotalAmount = request.TotalAmount,
                Status = OrderStatus.Pending,
                CreateAt = DateTime.Now,
                Description = request.GhiChu,
                EstimatedDeliveryDate = DateTime.Now.AddDays(new Random().Next(3, 6))
            };
            _context.OrderInfos.Add(order);

            // Thêm OrderDetail
            foreach (var item in request.OrderItems)
            {
                var productDetail = await _context.ProductDetails.FindAsync(item.ProductDetailId);
                if (productDetail == null)
                    throw new InvalidOperationException($"Không tìm thấy sản phẩm: {productDetail.Name}");

                if (productDetail.Quantity < item.Quantity)
                    throw new InvalidOperationException($"Sản phẩm {productDetail.Name} không đủ hàng.");

                // Nếu thanh toán MoMo thì mới trừ số lượng trong kho
                if (request.PaymentMethodCode == "momo")
                {
                    productDetail.Quantity -= item.Quantity;
                }

                _context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.Id,
                    ProductDetailId = item.ProductDetailId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }
            // Thêm PaymentMethod
            string pmName = request.PaymentMethodCode == "momo"
                ? "Thanh toán qua MoMo"
                : "Thanh toán khi nhận hàng (COD)";
            var paymentMethodId = await _context.PaymentMethods
                .Where(p => p.Name == pmName)
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync();

            if (paymentMethodId == null)
                throw new InvalidOperationException($"Không tìm thấy phương thức thanh toán: {pmName}");

            _context.OrderPaymentMethods.Add(new OrderPaymentMethod
            {
                OrderId = order.Id,
                PaymentMethodId = paymentMethodId.Value,
                PaymentAmount = request.TotalAmount
            });

            // Thêm ModeOfPayment
            string modeName = request.PaymentMethodCode == "momo" ? "Chuyển khoản" : "Tiền mặt";
            var modeOfPaymentId = await _context.ModeOfPayments
                .Where(m => m.Name == modeName)
                .Select(m => (Guid?)m.Id)
                .FirstOrDefaultAsync();

            if (modeOfPaymentId == null)
                throw new InvalidOperationException($"Không tìm thấy hình thức thanh toán: {modeName}");

            _context.ModeOfPaymentOrders.Add(new ModeOfPaymentOrder
            {
                OrderId = order.Id,
                ModeOfPaymentId = modeOfPaymentId.Value
            });
            // Voucher (nếu có chọn)
            if (request.VorcherId != null && request.VorcherId != Guid.Empty)
            {
                // Lấy voucher từ DB
                var voucher = await _context.Vouchers.FindAsync(request.VorcherId.Value);
                if (voucher == null)
                    throw new InvalidOperationException("Voucher không tồn tại.");

                // Kiểm tra trạng thái Active
                if (voucher.Status != VoucherStatus.Active)
                    throw new InvalidOperationException("Voucher không còn hiệu lực.");

                // Kiểm tra số lượng còn lại
                if (voucher.TotalUsageLimit <= 0)
                    throw new InvalidOperationException("Voucher đã hết số lượng sử dụng.");

                // Trừ số lượng voucher
                voucher.TotalUsageLimit -= 1;

                // Thêm bản ghi CustomerVoucher
                _context.CustomerVouchers.Add(new CustomerVoucher
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    VoucherId = request.VorcherId.Value,
                    UsedDate = DateTime.Now
                });
            }

            // Lưu tất cả
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OrderID { Id = order.Id };
        }

        public async Task<ThanhToanCartIdDto> GetCartViewModelAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username không được để trống");

            // Lấy CustomerId
            var customerId = await _context.Customers
                .Where(c => c.UserName == username)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (customerId == Guid.Empty)
            {
                return new ThanhToanCartIdDto
                {
                    CartItems = new List<CastCustomerIDDto>(),
                    Addresses = new List<AddressDto>(),
                    Vouchers = new List<VoucherDto>()
                };
            }

            // Lấy danh sách sản phẩm trong giỏ (không gọi lại hàm GetCurrenIDtAsync)
            var cartId = await _context.Carts
                .Where(c => c.CustomerId == customerId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            var cartItems = new List<CastCustomerIDDto>();
            if (cartId != Guid.Empty)
            {
                cartItems = await _context.CartItems
                    .Where(ci => ci.CartId == cartId)
                    .Include(ci => ci.ProductDetail)
                        .ThenInclude(pd => pd.Product)
                    .Include(ci => ci.ProductDetail.Color)
                    .Include(ci => ci.ProductDetail.Size)
                    .Include(ci => ci.ProductDetail.Images)
                    .Select(ci => new CastCustomerIDDto
                    {
                        Id = ci.Id,
                        ProductDetailId = ci.ProductDetailId,
                        productdetailcode = ci.ProductDetail.Code,
                        Quantity = ci.Quantity,
                        Price = ci.Price,
                        ProductName = ci.ProductDetail.Product.Name,
                        ColorName = ci.ProductDetail.Color.Name,
                        SizeName = ci.ProductDetail.Size.Name,
                        ImageUrl = ci.ProductDetail.Images.FirstOrDefault().Url ?? "/images/default.jpg"
                    })
                    .ToListAsync();
            }

            // Lấy danh sách địa chỉ
            var addresses = await _context.Addresses
                .Where(a => a.CustomerId == customerId)
                .Select(a => new AddressDto
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    PhoneNumber = a.PhoneNumber,
                    DetailAddress = a.DetailAddress,
                    Ward = a.Ward,
                    District = a.District,
                    Province = a.Province,
                    Stastus = a.Status
                })
                .ToListAsync();

            // Lấy danh sách voucher khả dụng nhưng chưa tồn tại trong bảng trung gian
            var vouchers = await _context.Vouchers
            .Where(v => v.Status == VoucherStatus.Active
&& v.EndDate >= DateTime.Now
                        && _context.CustomerVouchers
                            .Count(cv => cv.VoucherId == v.Id && cv.CustomerId == customerId) < v.MaxUsagePerCustomer && v.TotalUsageLimit > 0
                  )
                .Select(v => new VoucherDto
                {
                    Id = v.Id,
                    Code = v.Code,
                    Description = v.Description,
                    TotalUsageLimit = v.TotalUsageLimit,
                    DiscountValue = v.DiscountValue,
                    MinOrderAmount = v.MinOrderAmount,
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    DiscountType = v.DiscountType,
                    MaxUsagePerCustomer = v.MaxUsagePerCustomer

                })
                .ToListAsync();
            return new ThanhToanCartIdDto
            {
                CartItems = cartItems,
                Addresses = addresses,
                Vouchers = vouchers
            };
        }
        public async Task<MuangaycustomerIdDto> MuaNgayAddAsync(HttpContext ctx, MuaNgayCustomerRequest request, string username)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProductDetailcodeMuaNgay))
                throw new ArgumentException("Yêu cầu không hợp lệ");

            // Lấy customerId từ username
            var customerId = await _context.Customers
                .Where(c => c.UserName == username)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (customerId == Guid.Empty)
                throw new Exception("Không tìm thấy khách hàng.");

            // Lấy thông tin sản phẩm
            var product = await _context.ProductDetails
                .Include(x => x.Color)
                .Include(x => x.Size)
                .Include(x => x.Images)
                .Include(p => p.PromotionProducts)
                    .ThenInclude(pp => pp.Promotion)
                .FirstOrDefaultAsync(x => x.Code == request.ProductDetailcodeMuaNgay);

            if (product == null)
                throw new Exception($"Sản phẩm {request.ProductDetailcodeMuaNgay} không tồn tại");

            if (product.Quantity < 1)
                throw new Exception("Sản phẩm hiện đang hết hàng.");

            // Xác định giá áp dụng
            decimal priceToUse = product.Price;
            var currentPromotion = product.PromotionProducts
                 .FirstOrDefault(p => p.Promotion.Status == VoucherStatus.Active);
            if (currentPromotion != null && currentPromotion.Priceafterduction >= 0)
                priceToUse = currentPromotion.Priceafterduction;

            // Số lượng mua
            int quantityToBuy = request.QuantityMuaNgay > 0 ? request.QuantityMuaNgay : 1;
            if (quantityToBuy > product.Quantity)
                throw new Exception($"Số lượng yêu cầu ({quantityToBuy}) vượt quá tồn kho ({product.Quantity}).");
            // Tạo DTO trả về
            var buyNowItem = new MuangaycustomerIdDto
            {
                Id = Guid.NewGuid(),
                ProductDetailId = product.Id,
                ProductDetailcode = product.Code,
                Name = product.Name,
                Quantity = quantityToBuy,
                Price = priceToUse,
                Total = priceToUse * quantityToBuy,
                ImageUrl = product.Images.FirstOrDefault()?.Url ?? "default.jpg",
                ColorName = product.Color?.Name ?? "NO_COLOR",
                ColorCode = product.Color?.Code ?? "#000",
                SizeName = product.Size?.Name ?? "NO_SIZE",
            };

            // Lưu vào Session
            ctx.Session.SetString(SessionBuyNowKey, JsonConvert.SerializeObject(buyNowItem));

            return buyNowItem;
        }
        public async Task<MuangaycustomerIdDto> MuaNgayAsync(HttpContext ctx, string username)
        {
            // Lấy dữ liệu từ session
            var json = ctx.Session.GetString(SessionBuyNowKey);
            if (string.IsNullOrEmpty(json))
                return null; // Trả về null nếu không có dữ liệu trong session

            // Lấy customerId từ username
            var customerId = await _context.Customers
                .Where(c => c.UserName == username)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (customerId == Guid.Empty)
                throw new Exception("Không tìm thấy khách hàng.");

            // Lấy địa chỉ
            var addresses = await _context.Addresses
                .Where(a => a.CustomerId == customerId)
                .Select(a => new AddressDto
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    PhoneNumber = a.PhoneNumber,
                    DetailAddress = a.DetailAddress,
                    Ward = a.Ward,
                    District = a.District,
                    Province = a.Province,
                    Stastus = a.Status
                })
                .ToListAsync();

            // Lấy voucher khả dụng
            var vouchers = await _context.Vouchers
                .Where(v => v.Status == VoucherStatus.Active
                            && v.EndDate >= DateTime.Now
                            && _context.CustomerVouchers.Count(cv => cv.VoucherId == v.Id && cv.CustomerId == customerId) < v.MaxUsagePerCustomer && v.TotalUsageLimit > 0
                )
                .Select(v => new VoucherDto
                {
                    Id = v.Id,
                    Code = v.Code,
                    Description = v.Description,
                    TotalUsageLimit = v.TotalUsageLimit,
                    DiscountValue = v.DiscountValue,
                    MinOrderAmount = v.MinOrderAmount,
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    DiscountType = v.DiscountType,
                    MaxUsagePerCustomer = v.MaxUsagePerCustomer
                })
                .ToListAsync();

            // Deserialize json thành MuangaycustomerIdDto
            var buyNowItem = JsonConvert.DeserializeObject<MuangaycustomerIdDto>(json);

            // Cập nhật địa chỉ và voucher
            buyNowItem.AddressList = addresses;
            buyNowItem.VoucherList = vouchers;

            return buyNowItem;
        }
        public async Task RemoveCartItem(string username)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserName == username);

            if (customer == null)
                throw new Exception("Không tìm thấy khách hàng");
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.CustomerId == customer.Id);

            if (cart == null)
                throw new Exception("Không tìm thấy giỏ hàng");
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();
            if (cartItems.Any())
            {
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }
        }
    }
}