using API.Domain.DTOs;
using API.Domain.DTOs.MoMo;
using API.Domain.Mappers;
using API.Domain.Request.OrderRequest;
using API.Domain.Service.IService;
using API.DomainCusTomer.DTOs.ThanhToanCustomer;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
//using API.Domain.Services.IServices;
using System.Net;
using System.Security.Policy;

namespace DomainAPI.Service
{
    public class OrderService : IOrderService
    {
        private readonly DbContextApp _context;
        private readonly IProductDetailService _productDetailService;
      

        public OrderService(DbContextApp context, IProductDetailService productDetailService)
        {
            _context = context;
            _productDetailService = productDetailService;
            
        }

        public async Task<OrderDto> CreatePosOrderAsync(CreateOrderRequest request, Guid userId)
        {
            // Lấy ID của PaymentMethod "Thanh toán khi nhận hàng (COD)"
            var codPaymentMethodId = await _context.PaymentMethods
                .Where(pm => pm.Name == "Thanh toán khi nhận hàng (COD)")
                .Select(pm => pm.Id)
                .FirstOrDefaultAsync();

            if (codPaymentMethodId == Guid.Empty)
                throw new Exception("Không tìm thấy phương thức thanh toán COD.");

            // Gán tổng thanh toán bằng tổng đơn hàng
            var totalPaid = request.TotalAmount;

            var orderId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            // Nếu thông tin khách hàng trống thì gán giá trị mặc định
            if (string.IsNullOrWhiteSpace(request.CustomerName) &&
                string.IsNullOrWhiteSpace(request.PhoneNumber) &&
                string.IsNullOrWhiteSpace(request.Address))
            {
                request.CustomerName = "Khách vãng lai";
                request.PhoneNumber = "N/A"; // có thể đặt mặc định hoặc giữ trống
                request.Address = "N/A";
            }
            var order = new OrderInfo
            {
                Id = orderId,
                CreateAt = now,
                UpdateAt = now,
                CreateBy = userId,
                UpdateBy = userId,
                
                CustomerName = request.CustomerName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                
                
                ShippingFee = request.ShippingFee,
                TotalAmount = request.TotalAmount,
                Description = request.Description,
                Status = OrderStatus.Delivered,

                OrderDetails = request.OrderDetails.Select(d => new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductDetailId = d.ProductDetailId,
                    Quantity = d.Quantity,
                    Price = d.Price
                }).ToList(),

                // Luôn set PaymentMethod là COD
                OrderPaymentMethods = new List<OrderPaymentMethod>
        {
            new OrderPaymentMethod
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                PaymentMethodId = codPaymentMethodId,
                PaymentAmount = request.TotalAmount
            }
        },

                ModeOfPaymentOrders = request.ModeOfPayments?.Select(mop => new ModeOfPaymentOrder
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ModeOfPaymentId = mop.Id
                }).ToList() ?? new List<ModeOfPaymentOrder>(),

                BillHistories = new List<OrderHistory>
        {
            new OrderHistory
            {
                Id = Guid.NewGuid(),
                BillId = orderId,
                Description = "Tạo đơn hàng",
                amount = request.TotalAmount.ToString("N0"),
                createAt = now,
                updateAt = now
            }
        }
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.OrderInfos.Add(order);
                await _context.SaveChangesAsync();

                await _productDetailService.UpdateProductQuantityAfterOrderAsync(order.OrderDetails.ToList());

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return OrderMapper.ToDto(order);
        }






        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            try
            {
                var orders = await _context.OrderInfos
                    .Include(o => o.Customer)
                    .Include(o => o.CreateByNavigation)
                    .Include(o => o.UpdateByNavigation)
                    .Include(o => o.OrderDetails).ThenInclude(od => od.ProductDetail)
                    .Include(o => o.OrderPaymentMethods).ThenInclude(op => op.PaymentMethod)
                    .Include(o => o.BillHistories)
                    .ToListAsync();

                return orders.Select(OrderMapper.ToDto).OrderBy(p=>p.Status).ThenBy(p => p.CreateAt).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi load danh sách đơn hàng: " + ex);
                throw;
            }
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _context.OrderInfos
                .Include(o => o.Customer)
                .Include(o => o.CreateByNavigation)
                .Include(o => o.UpdateByNavigation)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductDetail)
                .Include(o => o.OrderPaymentMethods)
                    .ThenInclude(op => op.PaymentMethod)
                .Include(o => o.BillHistories)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            // ✅ Tính tổng tiền từ dữ liệu đã lưu (OrderDetails)
            decimal calculatedTotal = order.OrderDetails.Sum(od => (od.Price ?? 0) * (od.Quantity ?? 0))
                                        + (order.ShippingFee ?? 0);

            return new OrderDto
            {
                Id = order.Id,
                CreateAt = order.CreateAt,
                UpdateAt = order.UpdateAt,
                CreateBy = order.CreateBy,
                UpdateBy = order.UpdateBy,
                CreateByName = order.CreateByNavigation?.Name,
                UpdateByName = order.UpdateByNavigation?.Name,
                CustomerName = order.CustomerName,
                PhoneNumber = order.PhoneNumber,
                Address = order.Address,
                ShippingFee = order.ShippingFee,
                TotalAmount = calculatedTotal, // ✅ luôn đồng bộ
                Description = order.Description,
                Status = order.Status,
                Qrcode = order.Qrcode,
                EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailDto
                {
                    Id = od.Id,
                    ProductDetailId = od.ProductDetailId,
                    ProductName = od.ProductDetail?.Name ?? "Không xác định",
                    Quantity = od.Quantity,
                    Price = od.Price
                }).ToList(),
                BillHistories = order.BillHistories.Select(b => new OrderHistoryDto
                {
                    Id = b.Id,
                    Amount = b.amount,
                    Description = b.Description,
                    CreateAt = b.createAt
                    
                }).ToList()
            };
        }



        public async Task<bool> DeleteOrderAsync(Guid orderId)
        {
            var order = await _context.OrderInfos.FindAsync(orderId);
            if (order == null) return false;

            _context.OrderInfos.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, Guid updatedBy, string? reason = null)
        {
            var order = await _context.OrderInfos
                .Include(o => o.OrderDetails).Include(p=>p.OrderPaymentMethods).ThenInclude(p=>p.PaymentMethod)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new InvalidOperationException("Không tìm thấy đơn hàng");

            if ((int)status < (int)order.Status)
                throw new InvalidOperationException("Không thể cập nhật lùi trạng thái đơn hàng");

            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Đơn hàng đã ở trạng thái cuối, không thể cập nhật");
            if (order.Status == OrderStatus.Pending && status == OrderStatus.Confirmed)
            {
                foreach (var detail in order.OrderDetails)
                {
                    var productDetail = await _context.ProductDetails
                        .FirstOrDefaultAsync(p => p.Id == detail.ProductDetailId);

                    if (productDetail != null)
                    {
                        if (productDetail.Quantity < (detail.Quantity ?? 0))
                            throw new InvalidOperationException($"Sản phẩm {productDetail.Id} không đủ số lượng trong kho");

                        productDetail.Quantity -= (detail.Quantity ?? 0);
                    }
                }
            }
            
            // Trả hàng về kho khi cancel
            if (status == OrderStatus.Cancelled)
            {
                // Lấy phương thức thanh toán chính của đơn hàng
                var paymentMethodName = order.OrderPaymentMethods
                    .FirstOrDefault().PaymentMethod.Name;

               

                // Nếu KHÔNG phải Pending + COD thì mới hoàn hàng về kho
                if (!(order.Status == OrderStatus.Pending && paymentMethodName== "Thanh toán khi nhận hàng (COD)"))
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        var productDetail = await _context.ProductDetails
                            .FirstOrDefaultAsync(p => p.Id == detail.ProductDetailId);

                        if (productDetail != null)
                        {
                            productDetail.Quantity += (detail.Quantity ?? 0);
                        }
                    }
                }
            }


            order.Status = status;
            order.UpdateBy = updatedBy;
            order.UpdateAt = DateTime.Now;
            

            // 👇 Lưu lý do hủy vào BillHistory
            _context.OrderHistories.Add(new OrderHistory
            {
                Id = Guid.NewGuid(),
                BillId = orderId,
                Description = status == OrderStatus.Cancelled
                    ? $"Hủy đơn hàng. Lý do: {reason ?? "Không có lý do"}"
                    : $"Cập nhật trạng thái đơn hàng thành {status}",
                amount = order.TotalAmount?.ToString(),
                createAt = DateTime.Now,
                updateAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(int updatedCount, List<string> errors)> UpdateOrderStatusBulkAsync(
 List<Guid> orderIds, OrderStatus status, Guid updatedBy)
        {
            if (orderIds == null || !orderIds.Any())
                throw new InvalidOperationException("Danh sách đơn hàng không hợp lệ");

            var orders = await _context.OrderInfos
                .Include(o => o.OrderDetails)
                .Include(o => o.OrderPaymentMethods).ThenInclude(p => p.PaymentMethod)
                .Where(o => orderIds.Contains(o.Id))
                .OrderBy(o => o.CreateAt) // ✅ xử lý đơn cũ trước
                .ToListAsync();

            if (!orders.Any())
                throw new InvalidOperationException("Không tìm thấy đơn hàng nào");

            int updatedCount = 0;
            var errors = new List<string>();

            foreach (var order in orders)
            {
                // Không cho cập nhật lùi trạng thái
                if ((int)status < (int)order.Status)
                {
                    errors.Add($"Đơn hàng {order.Id} không thể cập nhật lùi trạng thái");
                    continue;
                }

                // Nếu đã ở trạng thái cuối
                if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                {
                    errors.Add($"Đơn hàng {order.Id} đã ở trạng thái cuối");
                    continue;
                }

                // Kiểm tra cập nhật theo thứ tự
                if ((int)status != (int)order.Status + 1 && status != OrderStatus.Cancelled)
                {
                    errors.Add($"Đơn hàng {order.Id} phải cập nhật theo thứ tự trạng thái: hiện tại {order.Status}, muốn cập nhật {status}");
                    continue;
                }

                // Pending → Confirmed: kiểm tra tồn kho
                if (order.Status == OrderStatus.Pending && status == OrderStatus.Confirmed)
                {
                    var paymentMethod = order.OrderPaymentMethods
                        .FirstOrDefault()?.PaymentMethod?.Name;

                    if (paymentMethod == "Thanh toán khi nhận hàng (COD)")
                    {
                        bool canConfirm = true;

                        foreach (var detail in order.OrderDetails)
                        {
                            var productDetail = await _context.ProductDetails
                                .FirstOrDefaultAsync(p => p.Id == detail.ProductDetailId);

                            if (productDetail == null)
                            {
                                errors.Add($"Không tìm thấy sản phẩm {detail.ProductDetailId} trong kho cho đơn {order.Id}");
                                canConfirm = false;
                                break;
                            }

                            if (productDetail.Quantity < (detail.Quantity ?? 0))
                            {
                                errors.Add($"Sản phẩm {productDetail.Name} không đủ số lượng trong kho cho đơn {order.Id}");
                                canConfirm = false;
                                break;
                            }
                        }

                        if (!canConfirm)
                            continue; // ❌ Bỏ qua đơn này, không trừ kho

                        // ✅ Nếu đủ → trừ kho ngay để đơn sau biết
                        foreach (var detail in order.OrderDetails)
                        {
                            var productDetail = await _context.ProductDetails
                                .FirstOrDefaultAsync(p => p.Id == detail.ProductDetailId);

                            productDetail.Quantity -= (detail.Quantity ?? 0);
                        }
                    }
                }

                // Nếu chuyển sang Cancelled => trả hàng về kho
                if (status == OrderStatus.Cancelled)
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        var productDetail = await _context.ProductDetails
                            .FirstOrDefaultAsync(p => p.Id == detail.ProductDetailId);

                        if (productDetail != null)
                        {
                            productDetail.Quantity += (detail.Quantity ?? 0);
                        }
                    }
                }

                // Cập nhật trạng thái
                order.Status = status;
                order.UpdateBy = updatedBy;
                order.UpdateAt = DateTime.Now;

                // Ghi log lịch sử
                _context.OrderHistories.Add(new OrderHistory
                {
                    Id = Guid.NewGuid(),
                    BillId = order.Id,
                    Description = $"Cập nhật trạng thái đơn hàng thành {status}",
                    amount = order.TotalAmount?.ToString(),
                    createAt = DateTime.Now,
                    updateAt = DateTime.Now
                });

                updatedCount++;
            }

            if (updatedCount > 0)
                await _context.SaveChangesAsync();

            return (updatedCount, errors);
        }









    }
}