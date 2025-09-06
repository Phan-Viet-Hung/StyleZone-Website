using API.DomainCusTomer.DTOs.QuanLyDonHangCustomerDto;
using API.DomainCusTomer.DTOs.ThanhToanCustomer;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;

namespace API.DomainCusTomer.Services
{
    public class ThanhToanCustomer : IThanhtoanCustomer
    {
        private readonly DbContextApp _context;

        public ThanhToanCustomer(DbContextApp context)
        {
            _context = context;
        }
        public async Task<OrderID> CreateGuestOrderAsync(OrderGuestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo OrderInfo
                var order = new OrderInfo
                {
                    Id = Guid.NewGuid(),
                    CustomerName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address + ", " + request.Ward + ", " + request.District + ", " + request.Province,
                    ShippingFee = request.ShippingFee,
                    TotalAmount = request.TotalAmount,
                    Status = OrderStatus.Pending,
                    CreateAt = DateTime.Now,
                    EstimatedDeliveryDate = DateTime.Now.AddDays(new Random().Next(3, 6))
                };
                _context.OrderInfos.Add(order);
                await _context.SaveChangesAsync();

                // 2. Thêm OrderDetail + kiểm tra tồn kho
                foreach (var item in request.OrderItems)
                {
                    var productDetail = await _context.ProductDetails.FindAsync(item.ProductDetailId);
                    if (productDetail == null)
                        throw new InvalidOperationException($"Không tìm thấy sản phẩm ID: {item.ProductDetailId}");

                    if (productDetail.Quantity < item.Quantity)
                        throw new InvalidOperationException($"Sản phẩm {productDetail.Name} không đủ hàng. Còn {productDetail.Quantity} cái.");

                    _context.OrderDetails.Add(new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductDetailId = item.ProductDetailId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });

                    // Nếu thanh toán MoMo thì trừ hàng
                    if (request.PaymentMethodCode == "momo")
                    {
                        productDetail.Quantity -= item.Quantity;
                        _context.ProductDetails.Update(productDetail);
                    }
                }
                await _context.SaveChangesAsync();

                // 3. Thêm OrderPaymentMethod
                string pmName = request.PaymentMethodCode == "momo"
                    ? "Thanh toán qua MoMo"
                    : "Thanh toán khi nhận hàng (COD)";
                var paymentMethodId = await _context.PaymentMethods
                    .Where(p => p.Name == pmName)
                    .Select(p => p.Id)
                    .FirstAsync();

                _context.OrderPaymentMethods.Add(new OrderPaymentMethod
                {
                    OrderId = order.Id,
                    PaymentMethodId = paymentMethodId,
                    PaymentAmount = request.TotalAmount
                });

                // 4. Thêm ModeOfPaymentOrder
                string modeName = request.PaymentMethodCode == "momo" ? "Chuyển khoản" : "Tiền mặt";
                var modeOfPaymentId = await _context.ModeOfPayments
                    .Where(m => m.Name == modeName)
                    .Select(m => m.Id)
                    .FirstAsync();

                _context.ModeOfPaymentOrders.Add(new ModeOfPaymentOrder
                {
                    OrderId = order.Id,
                    ModeOfPaymentId = modeOfPaymentId
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OrderID { Id = order.Id };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<QuanLyDonHangCustomerDto?> Tracuudonhang(string orderid)
        {
            if (string.IsNullOrWhiteSpace(orderid))
                return null; // Không nhập thì không trả gì cả

            var query = _context.OrderInfos
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductDetail)
                        .ThenInclude(pd => pd.Color)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductDetail)
                        .ThenInclude(pd => pd.Size)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductDetail)
                        .ThenInclude(pd => pd.Images);

            return await query
                .Where(o => o.Id.ToString() == orderid) // chỉ đúng ID mới lấy
                .Select(o => new QuanLyDonHangCustomerDto
                {
                    OrderId = o.Id,
                    CustomerId = o.CustomerId,
                    CustomerName = o.CustomerName,
                    Address = o.Address,
                    PhoneNumber = o.PhoneNumber,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    Details = o.OrderDetails.Select(d => new OrderDetail
                    {
                        Id = d.Id,
                        Quantity = d.Quantity,
                        Price = d.Price,
                        ProductDetailId = d.ProductDetailId,
                        ProductDetail = new ProductDetail
                        {
                            Id = d.ProductDetail.Id,
                            Name = d.ProductDetail.Name,
                            Price = d.ProductDetail.Price,
                            Color = d.ProductDetail.Color == null ? null : new Color
                            {
                                Id = d.ProductDetail.Color.Id,
                                Name = d.ProductDetail.Color.Name,
                                Code = d.ProductDetail.Color.Code
                            },
                            Size = d.ProductDetail.Size == null ? null : new Size
                            {
                                Id = d.ProductDetail.Size.Id,
                                Name = d.ProductDetail.Size.Name,
                                Code = d.ProductDetail.Size.Code
                            },
                            Images = d.ProductDetail.Images
                                .Select(img => new Image
                                {
                                    Id = img.Id,
                                    Url = img.Url,
                                    ProductDetailId = img.ProductDetailId
                                }).ToList()
                        }
                    }).ToList()
                })
                .FirstOrDefaultAsync(); // chỉ lấy đúng 1 đơn hàng
        }
    }
}
