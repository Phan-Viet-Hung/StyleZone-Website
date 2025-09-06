using API.DomainCusTomer.Config;
using API.DomainCusTomer.DTOs.QuanLyDonHangCustomerDto;
using API.DomainCusTomer.DTOs.ThongTinCaNhaCustomer;
using API.DomainCusTomer.ExTentions;
using API.DomainCusTomer.Request.ThongTinCaNhan;
using API.DomainCusTomer.Services.IServices;
using DAL_Empty.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace API.DomainCusTomer.Services
{
    public class DonMuaCustomerService : IDonMuaCustomerServices
    {
        private readonly DbContextApp _context;

        public DonMuaCustomerService(DbContextApp context)
        {
            _context = context;
        }
        public async Task<List<QuanLyDonHangCustomerDto>> ListDonHang(string username)
        {
            return await GetOrdersByStatus(username, null);
        }

        public Task<List<QuanLyDonHangCustomerDto>> ListDonHangPending(string username)
        {
            return GetOrdersByStatus(username, OrderStatus.Pending);
        }

        public Task<List<QuanLyDonHangCustomerDto>> ListDonHangConfirmed(string username)
        {
            return GetOrdersByStatus(username, OrderStatus.Confirmed);
        }

        public Task<List<QuanLyDonHangCustomerDto>> ListDonHangProcessing(string username)
        {
            return GetOrdersByStatus(username, OrderStatus.Processing);
        }

        public Task<List<QuanLyDonHangCustomerDto>> ListDonHangShipping(string username)
        {
            return GetOrdersByStatus(username, OrderStatus.Shipping);
        }

        public Task<List<QuanLyDonHangCustomerDto>> ListDonHangDelivered(string username)
        {
            return GetOrdersByStatus(username, OrderStatus.Delivered);
        }

        public Task<List<QuanLyDonHangCustomerDto>> ListDonHangCancelled(string username)
        {
            return GetOrdersByStatus(username, OrderStatus.Cancelled);
        }

        private async Task<List<QuanLyDonHangCustomerDto>> GetOrdersByStatus(string username, OrderStatus? status)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserName == username);

            if (customer == null)
                throw new Exception("Không tìm thấy khách hàng");

            var query = _context.OrderInfos
     .Include(o => o.OrderDetails)
         .ThenInclude(od => od.ProductDetail)
             .ThenInclude(pd => pd.Color)
     .Include(o => o.OrderDetails)
         .ThenInclude(od => od.ProductDetail)
             .ThenInclude(pd => pd.Size)
     .Where(o => o.CustomerId == customer.Id);

            // Lọc theo trạng thái nếu có truyền vào
            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return await query
                 .OrderByDescending(o => o.CreateAt)
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
                .ToListAsync();
        }


        public async Task<List<DonMuaCustomerDto>> GetOrders(string username, OrderStatus? status = null)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserName == username);

            if (customer == null)
                throw new Exception("Không tìm thấy khách hàng.");

            var query = _context.OrderInfos
                .Include(o => o.OrderDetails)
                .Where(o => o.CustomerId == customer.Id);
            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var orders = await query
                .OrderByDescending(o => o.CreateAt)
                .ToListAsync();

            var result = orders.Select(o => o.ToDonMuaCustomerDto()).ToList();

            return result;
        }






        public async Task<UpdateThongTinCaNhanDto> UpdateCustome(string username, ThongTinCaNhanRequest updatedCustomer)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserName == username);

            if (customer == null)
                throw new Exception("Không tìm thấy khách hàng.");

            customer.Fullname = updatedCustomer.Fullname;
            customer.Email = updatedCustomer.Email;
            customer.UserName = updatedCustomer.UserName;
            customer.PhoneNumber = updatedCustomer.PhoneNumber;
            customer.Birthday = updatedCustomer.Birthday;
            customer.Gender = updatedCustomer.Gender;
            customer.UpdateAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return customer.ToUpdateThongTinCaNhanDto();
        }
        public async Task<UpdateThongTinCaNhanDto> DetailsCustome(string username)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserName == username);

            return customer.ToUpdateThongTinCaNhanDto();
        }


        public async Task UpdatePassWord(RePassDtoCustomer rePassDtoCustomer, string username)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserName == username);
            if (customer == null)
                throw new Exception("Không tìm thấy khách hàng.");
            if (!rePassDtoCustomer.OldPassword.VerifyPassword(customer.Password))
                throw new Exception("Mật khẩu cũ không chính xác.");
            if (rePassDtoCustomer.OldPassword == rePassDtoCustomer.NewPassword)
                throw new Exception("Mật khẩu mới không được trùng với mật khẩu cũ.");


            customer.Password = rePassDtoCustomer.NewPassword.HashPassword();
            customer.UpdateAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task AddDiaChi(string username, DiachiCustomerDto newAddress)
        {
            var customer = await _context.Customers
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.UserName == username);

            if (customer == null)
                throw new Exception("Không tìm thấy khách hàng.");

            // Kiểm tra trùng địa chỉ
            var isExist = customer.Addresses.Any(a =>
                  string.Equals(a.DetailAddress?.Trim(), newAddress.DetailAddress?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                  string.Equals(a.Province?.Trim(), newAddress.Province?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                  string.Equals(a.District?.Trim(), newAddress.District?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                  string.Equals(a.Ward?.Trim(), newAddress.Ward?.Trim(), StringComparison.OrdinalIgnoreCase)
              );

            if (isExist)
                throw new Exception("Địa chỉ này đã được thêm.");


            // Xoá trạng thái của tất cả địa chỉ hiện tại (cập nhật thành null)
            foreach (var address in customer.Addresses)
            {
                address.Status = null;

            }

            // Tạo địa chỉ mới
            var addressEntity = new Address
            {
                Id = Guid.NewGuid(),
                Province = newAddress.Province,
                District = newAddress.District,
                Ward = newAddress.Ward,
                PhoneNumber = newAddress.PhoneNumber,
                DetailAddress = newAddress.DetailAddress,
                FullName = newAddress.FullName,
                Status = "Mặc định",
                CustomerId = customer.Id
            };

            _context.Addresses.Add(addressEntity);

            await _context.SaveChangesAsync();
        }
        public async Task UpdateStastusDiaChi(Guid Id, string username)
        {
            var customer = await _context.Customers
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.UserName == username);

            if (customer == null)
                throw new Exception("Không tìm thấy khách hàng");


            var addressToUpdate = customer.Addresses.FirstOrDefault(a => a.Id == Id);
            if (addressToUpdate == null)
                throw new Exception("Không tìm thấy địa chỉ");
            foreach (var address in customer.Addresses)
            {
                address.Status = null;
            }
            addressToUpdate.Status = "Mặc định";
            //addressToUpdate.IsDefault = true;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDiaChi(Guid Id, DiachiCustomerDto address)
        {
            var existingAddress = _context.Addresses.FirstOrDefault(a => a.Id == Id);
            if (existingAddress == null)
            {
                throw new Exception("Không tìm thấy địa chỉ để cập nhật.");
            }

            // Cập nhật thông tin địa chỉ
            existingAddress.Province = address.Province;
            existingAddress.District = address.District;
            existingAddress.Ward = address.Ward;
            existingAddress.PhoneNumber = address.PhoneNumber;
            existingAddress.DetailAddress = address.DetailAddress;
            existingAddress.FullName = address.FullName;
            //existingAddress.Status = address.Status;

            _context.Addresses.Update(existingAddress);

            await _context.SaveChangesAsync();
        }


        public async Task<List<Address>> ListDiaChiCustomer(string username)
        {
            var customer = await _context.Customers
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.UserName == username);

            return customer?.Addresses?.OrderByDescending(a => a.Status == "Mặc định").ToList() ?? new List<Address>();
        }

        public async Task RemoveDiaChi(Guid id)
        {
            var a = await _context.Addresses.FirstOrDefaultAsync(x => x.Id == id);
            _context.Addresses.Remove(a);
            _context.SaveChangesAsync();
        }

        public async Task CancelOrderAsync(Guid orderId, string username, string Decription)
        {
            var order = await _context.OrderInfos
              .Include(o => o.Customer)
              .Include(o => o.OrderDetails)
                  .ThenInclude(oi => oi.ProductDetail)
              .Include(o => o.OrderPaymentMethods)
                  .ThenInclude(opm => opm.PaymentMethod)
              .FirstOrDefaultAsync(o => o.Id == orderId && o.Customer.UserName == username);

            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng");

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
                throw new Exception("Đơn hàng này không thể hủy");

            var paymentMethodName = order.OrderPaymentMethods
                .Select(opm => opm.PaymentMethod.Name)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(paymentMethodName))
                throw new Exception("Không xác định được phương thức thanh toán");

            // ✅ Nếu là MoMo thì cộng lại tồn kho
            if (paymentMethodName.Equals("Thanh toán qua MoMo", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in order.OrderDetails)
                {
                    if (item.ProductDetail != null)
                    {
                        item.ProductDetail.Quantity += item.Quantity;
                    }
                }
            }

            // Cập nhật trạng thái
            order.Status = OrderStatus.Cancelled;
            order.Description = Decription;
            await _context.SaveChangesAsync();

            // ✅ Trả về thông báo thay vì throw
            if (paymentMethodName.Equals("Thanh toán khi nhận hàng (COD)", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Đơn hàng đã được hủy thành công.");
            }
            else if (paymentMethodName.Equals("Thanh toán qua MoMo", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Đơn hàng đã được hủy. Vui lòng chụp lại lịch sử giao dịch và gửi cho chúng tôi để được hoàn tiền.");
            }
            else
            {
                throw new Exception($"Đơn hàng đã được hủy. (Phương thức thanh toán: {paymentMethodName})");
            }
        }

        public async Task CancelOrderAsyncGuest(Guid orderId, string Decription)
        {
            var order = await _context.OrderInfos
                 .Include(o => o.Customer)
                 .Include(o => o.OrderDetails)
                     .ThenInclude(oi => oi.ProductDetail)
                 .Include(o => o.OrderPaymentMethods)
                     .ThenInclude(opm => opm.PaymentMethod)
                 .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng");

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
                throw new Exception("Đơn hàng này không thể hủy");

            var paymentMethodName = order.OrderPaymentMethods
                .Select(opm => opm.PaymentMethod.Name)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(paymentMethodName))
                throw new Exception("Không xác định được phương thức thanh toán");

            // ✅ Nếu là MoMo thì cộng lại tồn kho
            if (paymentMethodName.Equals("Thanh toán qua MoMo", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in order.OrderDetails)
                {
                    if (item.ProductDetail != null)
                    {
                        item.ProductDetail.Quantity += item.Quantity;
                    }
                }
            }

            // Cập nhật trạng thái
            order.Status = OrderStatus.Cancelled;
            order.Description = Decription;
            await _context.SaveChangesAsync();

            // ✅ Trả về thông báo thay vì throw
            if (paymentMethodName.Equals("Thanh toán khi nhận hàng (COD)", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Đơn hàng đã được hủy thành công.");
            }
            else if (paymentMethodName.Equals("Thanh toán qua MoMo", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Đơn hàng đã được hủy. Vui lòng chụp lại lịch sử giao dịch và gửi cho chúng tôi để được hoàn tiền.");
            }
            else
            {
                throw new Exception($"Đơn hàng đã được hủy. (Phương thức thanh toán: {paymentMethodName})");
            }
        }
    }

}
