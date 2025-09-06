using API.Domain.DTOs;
using API.Domain.Request.OrderRequest;
using DAL_Empty.Models;

namespace API.Domain.Mappers
{
    public static class OrderMapper
    {
        public static OrderInfo ToEntity(CreateOrderRequest request, Guid createdBy, Guid codPaymentMethodId)
        {
            var orderId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            return new OrderInfo
            {
                Id = orderId,
                CreateAt = now,
                CreateBy = createdBy,
                UpdateAt = now,
                UpdateBy = createdBy,
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
                    ProductDetailId = d.ProductDetailId,
                    Quantity = d.Quantity,
                    Price = d.Price,
                    OrderId = orderId
                }).ToList(),

                // Luôn tạo 1 phương thức thanh toán COD
                OrderPaymentMethods = new List<OrderPaymentMethod>
        {
            new OrderPaymentMethod
            {
                Id = Guid.NewGuid(),
                PaymentMethodId = codPaymentMethodId,
                OrderId = orderId,
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
        }


        public static OrderDto ToDto(OrderInfo entity)
        {
            return new OrderDto
            {
                Id = entity.Id,
                CreateAt = entity.CreateAt,
                CreateBy = entity.CreateBy,
                UpdateAt = entity.UpdateAt,
                UpdateBy = entity.UpdateBy,
                CustomerId = entity.CustomerId,
                CustomerName = entity.CustomerName,
                PhoneNumber = entity.PhoneNumber,
                Address = entity.Address,
                ShippingFee = entity.ShippingFee,
                TotalAmount = entity.TotalAmount,
                Description = entity.Description,
                Status = entity.Status,
                Qrcode = entity.Qrcode,
                EstimatedDeliveryDate = entity.EstimatedDeliveryDate,
                Customer = entity.Customer == null ? null : new CustomerDto
                {
                    Id = entity.Customer.Id,
                    Fullname = entity.Customer.Fullname,
                    PhoneNumber = entity.Customer.PhoneNumber,
                    Email = entity.Customer.Email,
                    Birthday = entity.Customer.Birthday,
                    Gender = entity.Customer.Gender
                },
                CreateByNavigation = entity.CreateByNavigation == null ? null : new AccountDto
                {
                    Id = entity.CreateByNavigation.Id,
                    Name = entity.CreateByNavigation.Name,
                    UserName = entity.CreateByNavigation.UserName,
                    PhoneNumber = entity.CreateByNavigation.PhoneNumber,
                    Email = entity.CreateByNavigation.Email
                },
                UpdateByNavigation = entity.UpdateByNavigation == null ? null : new AccountDto
                {
                    Id = entity.UpdateByNavigation.Id,
                    Name = entity.UpdateByNavigation.Name,
                    UserName = entity.UpdateByNavigation.UserName,
                    PhoneNumber = entity.UpdateByNavigation.PhoneNumber,
                    Email = entity.UpdateByNavigation.Email
                },
                OrderDetails = entity.OrderDetails.Select(d => new OrderDetailDto
                {
                    Id = d.Id,
                    ProductDetailId = d.ProductDetailId,
                    ProductName = d.ProductDetail?.Name ?? "Không xác định",
                    Quantity = d.Quantity,
                    Price = d.Price
                }).ToList(),
                OrderPaymentMethods = entity.OrderPaymentMethods.Select(p => new OrderPaymentMethodDto
                {
                    Id = p.Id,
                    PaymentMethodId = p.PaymentMethodId,
                    PaymentMethodName = p.PaymentMethod?.Name,
                    PaymentAmount = p.PaymentAmount
                }).ToList(),
                ModeOfPaymentOrders = entity.ModeOfPaymentOrders.Select(m => new ModeOfPaymentOrderDto
                {
                    Id = m.Id,
                    ModeOfPaymentId = m.ModeOfPaymentId
                }).ToList(),
                BillHistories = entity.BillHistories.Select(h => new OrderHistoryDto
                {
                    Id = h.Id,
                    Description = h.Description,
                    Amount = h.amount,
                    CreateAt = h.createAt
                }).ToList()
            };
        }
    }
}
